using Microsoft.Extensions.Options;
using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Menus;
using SunAdmin.Domain.Constants;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;
using SunAdmin.Infrastructure.Options;

namespace SunAdmin.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    IFreeSql freeSql,
    IPasswordHasher passwordHasher,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<SeedOptions> seedOptions) : IDatabaseInitializer
{
    private const string ReadonlyAdminRoleCode = "readonly_admin";
    private const string UserAdminRoleCode = "user_admin";

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (databaseOptions.Value.DisableInitializer)
        {
            return;
        }

        if (databaseOptions.Value.SyncStructure)
        {
            await Task.Run(() =>
            {
                freeSql.CodeFirst.SyncStructure<User>();
                freeSql.CodeFirst.SyncStructure<Role>();
                freeSql.CodeFirst.SyncStructure<Menu>();
                freeSql.CodeFirst.SyncStructure<Department>();
                freeSql.CodeFirst.SyncStructure<Position>();
                freeSql.CodeFirst.SyncStructure<OperationLog>();
                freeSql.CodeFirst.SyncStructure<LoginLog>();
                freeSql.CodeFirst.SyncStructure<LoginSession>();
                freeSql.CodeFirst.SyncStructure<SystemSetting>();
                freeSql.CodeFirst.SyncStructure<UserRole>();
                freeSql.CodeFirst.SyncStructure<RoleMenu>();
                freeSql.CodeFirst.SyncStructure<DataDictionary>();
                freeSql.CodeFirst.SyncStructure<DataDictionaryItem>();
                freeSql.CodeFirst.SyncStructure<SystemNotification>();
                freeSql.CodeFirst.SyncStructure<FileResource>();
                freeSql.CodeFirst.SyncStructure<ExportTask>();
                freeSql.CodeFirst.SyncStructure<CodeGenerationTemplate>();
                freeSql.CodeFirst.SyncStructure<EntityChangeLog>();
            }, cancellationToken);
        }

        if (databaseOptions.Value.SeedData)
        {
            await SeedAsync(cancellationToken);
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        var role = await EnsureRoleAsync(
            SystemRoleCodes.SuperAdmin,
            "超级管理员",
            "内置角色，拥有全部接口权限。",
            cancellationToken);

        var seed = seedOptions.Value;
        var admin = await freeSql.Select<User>().Where(x => x.UserName == seed.AdminUserName).FirstAsync(cancellationToken);
        if (admin is null)
        {
            admin = new User
            {
                UserName = seed.AdminUserName,
                DisplayName = "系统管理员",
                Email = seed.AdminEmail,
                PasswordHash = passwordHasher.HashPassword(seed.AdminPassword),
                IsBuiltIn = true
            };
            admin.Id = await freeSql.Insert(admin).ExecuteIdentityAsync(cancellationToken);
        }
        else
        {
            admin.DisplayName = "系统管理员";
            admin.Email = seed.AdminEmail;
            admin.IsBuiltIn = true;
            admin.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<User>().SetSource(admin).ExecuteAffrowsAsync(cancellationToken);
        }

        var menus = await EnsureMenusAsync(cancellationToken);
        await EnsureDepartmentsAsync(cancellationToken);
        await EnsurePositionsAsync(cancellationToken);
        await EnsureSettingsAsync(cancellationToken);
        await EnsureDictionariesAsync(cancellationToken);
        await EnsureCodeGenerationTemplatesAsync(cancellationToken);
        var readonlyRole = await EnsureRoleAsync(
            ReadonlyAdminRoleCode,
            "只读管理员",
            "示例角色，可查看用户、角色、菜单、组织、日志、会话和配置。",
            cancellationToken);
        var userAdminRole = await EnsureRoleAsync(
            UserAdminRoleCode,
            "用户管理员",
            "示例角色，可管理用户、部门和岗位，不能维护角色和菜单。",
            cancellationToken);

        if (!await freeSql.Select<UserRole>().Where(x => x.UserId == admin.Id && x.RoleId == role.Id).AnyAsync(cancellationToken))
        {
            await freeSql.Insert(new UserRole { UserId = admin.Id, RoleId = role.Id }).ExecuteAffrowsAsync(cancellationToken);
        }

        await EnsureRoleMenusAsync(role.Id, menus.Select(x => x.Id), cancellationToken);
        await EnsureRoleMenusAsync(
            readonlyRole.Id,
            menus.Where(x =>
                    x.Type is MenuType.Directory or MenuType.Page ||
                    x.PermissionCode is SystemPermissionCodes.UserView or SystemPermissionCodes.RoleView or SystemPermissionCodes.MenuView or SystemPermissionCodes.DepartmentView or SystemPermissionCodes.PositionView
                        or SystemPermissionCodes.OperationLogView or SystemPermissionCodes.LoginLogView or SystemPermissionCodes.SessionView or SystemPermissionCodes.SettingView
                        or SystemPermissionCodes.DictionaryView or SystemPermissionCodes.NotificationView or SystemPermissionCodes.FileView or SystemPermissionCodes.ExportView
                        or SystemPermissionCodes.CodeGenerationView or SystemPermissionCodes.EntityChangeLogView)
                .Select(x => x.Id),
            cancellationToken);
        await EnsureRoleMenusAsync(
            userAdminRole.Id,
            menus.Where(x =>
                    x.Type is MenuType.Directory ||
                    x.RoutePath is "/dashboard" or "/users" or "/departments" or "/positions" ||
                    x.PermissionCode is SystemPermissionCodes.UserView or SystemPermissionCodes.UserCreate or SystemPermissionCodes.UserUpdate or SystemPermissionCodes.UserDelete or SystemPermissionCodes.DepartmentView or SystemPermissionCodes.PositionView)
                .Select(x => x.Id),
            cancellationToken);
    }

    private async Task<Role> EnsureRoleAsync(string code, string name, string description, CancellationToken cancellationToken)
    {
        var existing = await freeSql.Select<Role>().Where(x => x.Code == code).FirstAsync(cancellationToken);
        if (existing is not null)
        {
            existing.Name = name;
            existing.Description = description;
            existing.IsBuiltIn = true;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<Role>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return existing;
        }

        var role = new Role
        {
            Code = code,
            Name = name,
            Description = description,
            IsBuiltIn = true
        };
        role.Id = await freeSql.Insert(role).ExecuteIdentityAsync(cancellationToken);
        return role;
    }

    private async Task EnsureRoleMenusAsync(long roleId, IEnumerable<long> menuIds, CancellationToken cancellationToken)
    {
        foreach (var menuId in menuIds.Distinct())
        {
            if (!await freeSql.Select<RoleMenu>().Where(x => x.RoleId == roleId && x.MenuId == menuId).AnyAsync(cancellationToken))
            {
                await freeSql.Insert(new RoleMenu { RoleId = roleId, MenuId = menuId }).ExecuteAffrowsAsync(cancellationToken);
            }
        }
    }

    private async Task<IReadOnlyList<Menu>> EnsureMenusAsync(CancellationToken cancellationToken)
    {
        var savedByCode = new Dictionary<string, Menu>();
        var saved = new List<Menu>();

        foreach (var descriptor in SystemPageRegistry.All)
        {
            long? parentId = descriptor.ParentCode is not null && savedByCode.TryGetValue(descriptor.ParentCode, out var parent)
                ? parent.Id
                : null;
            var menu = await EnsureMenuAsync(new Menu
            {
                Name = descriptor.Name,
                Type = descriptor.Type,
                RoutePath = descriptor.RoutePath,
                Component = descriptor.Component,
                Icon = descriptor.Icon,
                PermissionCode = descriptor.PermissionCode,
                SortOrder = descriptor.SortOrder,
                IsBuiltIn = true
            }, parentId, cancellationToken);
            savedByCode[descriptor.Code] = menu;
            saved.Add(menu);
        }

        return saved;
    }

    private async Task EnsureDepartmentsAsync(CancellationToken cancellationToken)
    {
        var headquarters = await EnsureDepartmentAsync(new Department
        {
            Code = "HQ",
            Name = "总部",
            Leader = "系统管理员",
            SortOrder = 10,
            IsBuiltIn = true
        }, null, cancellationToken);

        await EnsureDepartmentAsync(new Department
        {
            Code = "OPS",
            Name = "运营部",
            SortOrder = 20,
            IsBuiltIn = true
        }, headquarters.Id, cancellationToken);

        await EnsureDepartmentAsync(new Department
        {
            Code = "TECH",
            Name = "技术部",
            SortOrder = 30,
            IsBuiltIn = true
        }, headquarters.Id, cancellationToken);
    }

    private async Task<Department> EnsureDepartmentAsync(Department definition, long? parentId, CancellationToken cancellationToken)
    {
        var existing = await freeSql.Select<Department>().Where(x => x.Code == definition.Code).FirstAsync(cancellationToken);
        if (existing is not null)
        {
            existing.ParentId = parentId;
            existing.Name = definition.Name;
            existing.Leader = definition.Leader;
            existing.SortOrder = definition.SortOrder;
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<Department>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return existing;
        }

        definition.ParentId = parentId;
        definition.Id = await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
        return definition;
    }

    private async Task EnsurePositionsAsync(CancellationToken cancellationToken)
    {
        await EnsurePositionAsync(new Position { Code = "ADMIN", Name = "系统管理员", SortOrder = 10, IsBuiltIn = true }, cancellationToken);
        await EnsurePositionAsync(new Position { Code = "OPS_MANAGER", Name = "运营主管", SortOrder = 20, IsBuiltIn = true }, cancellationToken);
        await EnsurePositionAsync(new Position { Code = "ENGINEER", Name = "工程师", SortOrder = 30, IsBuiltIn = true }, cancellationToken);
    }

    private async Task EnsurePositionAsync(Position definition, CancellationToken cancellationToken)
    {
        var existing = await freeSql.Select<Position>().Where(x => x.Code == definition.Code).FirstAsync(cancellationToken);
        if (existing is not null)
        {
            existing.Name = definition.Name;
            existing.SortOrder = definition.SortOrder;
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<Position>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return;
        }

        await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
    }

    private async Task EnsureSettingsAsync(CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            new SystemSetting
            {
                Key = "security.password.minLength",
                Name = "密码最小长度",
                Value = "8",
                Description = "用于前端提示和后端校验基线，建议不低于 8。"
            },
            new SystemSetting
            {
                Key = "security.password.requireDigit",
                Name = "密码需要数字",
                Value = "true",
                Description = "新建、重置和修改密码时是否至少包含一个数字。"
            },
            new SystemSetting
            {
                Key = "security.password.requireUppercase",
                Name = "密码需要大写字母",
                Value = "true",
                Description = "新建、重置和修改密码时是否至少包含一个大写字母。"
            },
            new SystemSetting
            {
                Key = "security.password.requireLowercase",
                Name = "密码需要小写字母",
                Value = "true",
                Description = "新建、重置和修改密码时是否至少包含一个小写字母。"
            },
            new SystemSetting
            {
                Key = "security.password.requireNonAlphanumeric",
                Name = "密码需要特殊字符",
                Value = "false",
                Description = "新建、重置和修改密码时是否至少包含一个非字母数字字符。"
            },
            new SystemSetting
            {
                Key = "security.password.forceChangeAfterReset",
                Name = "重置后强制改密",
                Value = "true",
                Description = "管理员重置用户密码后，用户下次登录必须修改密码。"
            },
            new SystemSetting
            {
                Key = "system.name",
                Name = "系统名称",
                Value = "sun-admin",
                Description = "后台系统展示名称。"
            }
        };

        foreach (var definition in definitions)
        {
            var existing = await freeSql.Select<SystemSetting>().Where(x => x.Key == definition.Key).FirstAsync(cancellationToken);
            if (existing is not null)
            {
                existing.Name = definition.Name;
                existing.Description = definition.Description;
                existing.UpdatedAt = DateTime.UtcNow;
                await freeSql.Update<SystemSetting>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
                continue;
            }

            await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
        }
    }

    private async Task EnsureDictionariesAsync(CancellationToken cancellationToken)
    {
        var status = await EnsureDictionaryAsync(new DataDictionary
        {
            Code = "record_status",
            Name = "记录状态",
            Description = "通用启用/禁用状态。",
            IsBuiltIn = true
        }, cancellationToken);
        await EnsureDictionaryItemAsync(status.Id, new DataDictionaryItem { Label = "启用", Value = "Enabled", SortOrder = 1, IsBuiltIn = true }, cancellationToken);
        await EnsureDictionaryItemAsync(status.Id, new DataDictionaryItem { Label = "禁用", Value = "Disabled", SortOrder = 2, IsBuiltIn = true }, cancellationToken);
    }

    private async Task<DataDictionary> EnsureDictionaryAsync(DataDictionary definition, CancellationToken cancellationToken)
    {
        var existing = await freeSql.Select<DataDictionary>().Where(x => x.Code == definition.Code).FirstAsync(cancellationToken);
        if (existing is not null)
        {
            existing.Name = definition.Name;
            existing.Description = definition.Description;
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<DataDictionary>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return existing;
        }

        definition.Id = await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
        return definition;
    }

    private async Task EnsureDictionaryItemAsync(long dictionaryId, DataDictionaryItem definition, CancellationToken cancellationToken)
    {
        var existing = await freeSql.Select<DataDictionaryItem>().Where(x => x.DictionaryId == dictionaryId && x.Value == definition.Value).FirstAsync(cancellationToken);
        if (existing is not null)
        {
            existing.Label = definition.Label;
            existing.SortOrder = definition.SortOrder;
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<DataDictionaryItem>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return;
        }

        definition.DictionaryId = dictionaryId;
        await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
    }

    private async Task EnsureCodeGenerationTemplatesAsync(CancellationToken cancellationToken)
    {
        await EnsureCodeGenerationTemplateAsync(new CodeGenerationTemplate
        {
            Name = "后端 DTO 模板",
            TemplateKey = "backend.dto",
            TargetKind = "backend",
            Content = """
                namespace {{Namespace}};

                public sealed record {{EntityName}}Dto(
                    long Id,
                    string Name,
                    string Status);

                public sealed record Create{{EntityName}}Request(
                    string Name);

                public sealed record Update{{EntityName}}Request(
                    string Name,
                    string Status);
                """,
            IsBuiltIn = true
        }, cancellationToken);
        await EnsureCodeGenerationTemplateAsync(new CodeGenerationTemplate
        {
            Name = "前端列表页模板",
            TemplateKey = "frontend.list",
            TargetKind = "frontend",
            Content = """
                import { useEffect, useState } from 'react';
                import { Table } from 'antd';

                export default function {{EntityName}}ListPage() {
                  const [items, setItems] = useState<{{EntityName}}Dto[]>([]);

                  useEffect(() => {
                    {{serviceName}}.list().then(setItems);
                  }, []);

                  return (
                    <Table
                      rowKey="id"
                      dataSource={items}
                      columns={[
                        { title: '名称', dataIndex: 'name' },
                        { title: '状态', dataIndex: 'status' },
                      ]}
                    />
                  );
                }
                """,
            IsBuiltIn = true
        }, cancellationToken);
    }

    private async Task EnsureCodeGenerationTemplateAsync(CodeGenerationTemplate definition, CancellationToken cancellationToken)
    {
        var existing = await freeSql.Select<CodeGenerationTemplate>().Where(x => x.TemplateKey == definition.TemplateKey).FirstAsync(cancellationToken);
        if (existing is not null)
        {
            existing.Name = definition.Name;
            existing.TargetKind = definition.TargetKind;
            if (string.IsNullOrWhiteSpace(existing.Content) || existing.Content.Contains("TODO", StringComparison.OrdinalIgnoreCase))
            {
                existing.Content = definition.Content;
            }
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<CodeGenerationTemplate>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return;
        }

        await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
    }

    private async Task<Menu> EnsureMenuAsync(Menu definition, long? parentId, CancellationToken cancellationToken)
    {
        var existing = await FindExistingMenuAsync(definition, cancellationToken);

        if (existing is not null)
        {
            existing.ParentId = parentId;
            existing.Name = definition.Name;
            existing.Type = definition.Type;
            existing.RoutePath = definition.RoutePath;
            existing.Component = definition.Component;
            existing.Icon = definition.Icon;
            existing.PermissionCode = definition.PermissionCode;
            existing.SortOrder = definition.SortOrder;
            existing.Status = definition.Status;
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<Menu>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return existing;
        }

        definition.ParentId = parentId;
        definition.Id = await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
        return definition;
    }

    private async Task<Menu?> FindExistingMenuAsync(Menu definition, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(definition.PermissionCode))
        {
            return await freeSql.Select<Menu>().Where(x => x.PermissionCode == definition.PermissionCode).FirstAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(definition.RoutePath))
        {
            return await freeSql.Select<Menu>().Where(x => x.RoutePath == definition.RoutePath && x.Type == definition.Type).FirstAsync(cancellationToken);
        }

        if (definition.Name == "系统管理")
        {
            return await freeSql.Select<Menu>().Where(x => x.Type == MenuType.Directory && (x.Name == "系统管理" || x.Name == "System")).FirstAsync(cancellationToken);
        }

        return await freeSql.Select<Menu>().Where(x => x.Name == definition.Name && x.Type == definition.Type).FirstAsync(cancellationToken);
    }
}
