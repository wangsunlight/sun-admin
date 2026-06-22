using Microsoft.Extensions.Options;
using SunAdmin.Application.Abstractions;
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
                        or SystemPermissionCodes.OperationLogView or SystemPermissionCodes.LoginLogView or SystemPermissionCodes.SessionView or SystemPermissionCodes.SettingView)
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
        var definitions = new[]
        {
            new Menu { Name = "系统管理", Type = MenuType.Directory, SortOrder = 10, IsBuiltIn = true },
            new Menu { Name = "工作台", Type = MenuType.Page, RoutePath = "/dashboard", Component = "dashboard/DashboardPage", SortOrder = 11, IsBuiltIn = true },
            new Menu { Name = "用户管理", Type = MenuType.Page, RoutePath = "/users", Component = "users/UserManagementPage", SortOrder = 12, IsBuiltIn = true },
            new Menu { Name = "角色管理", Type = MenuType.Page, RoutePath = "/roles", Component = "roles/RoleManagementPage", SortOrder = 13, IsBuiltIn = true },
            new Menu { Name = "部门管理", Type = MenuType.Page, RoutePath = "/departments", Component = "departments/DepartmentManagementPage", SortOrder = 14, IsBuiltIn = true },
            new Menu { Name = "岗位管理", Type = MenuType.Page, RoutePath = "/positions", Component = "positions/PositionManagementPage", SortOrder = 15, IsBuiltIn = true },
            new Menu { Name = "菜单管理", Type = MenuType.Page, RoutePath = "/menus", Component = "menus/MenuManagementPage", SortOrder = 16, IsBuiltIn = true },
            new Menu { Name = "日志审计", Type = MenuType.Page, RoutePath = "/logs", Component = "logs/LogManagementPage", SortOrder = 17, IsBuiltIn = true },
            new Menu { Name = "在线会话", Type = MenuType.Page, RoutePath = "/sessions", Component = "sessions/SessionManagementPage", SortOrder = 18, IsBuiltIn = true },
            new Menu { Name = "系统配置", Type = MenuType.Page, RoutePath = "/settings", Component = "settings/SettingManagementPage", SortOrder = 19, IsBuiltIn = true },
            new Menu { Name = "查看用户", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserView, SortOrder = 101, IsBuiltIn = true },
            new Menu { Name = "新建用户", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserCreate, SortOrder = 102, IsBuiltIn = true },
            new Menu { Name = "编辑用户", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserUpdate, SortOrder = 103, IsBuiltIn = true },
            new Menu { Name = "删除用户", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserDelete, SortOrder = 104, IsBuiltIn = true },
            new Menu { Name = "查看角色", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleView, SortOrder = 201, IsBuiltIn = true },
            new Menu { Name = "新建角色", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleCreate, SortOrder = 202, IsBuiltIn = true },
            new Menu { Name = "编辑角色", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleUpdate, SortOrder = 203, IsBuiltIn = true },
            new Menu { Name = "删除角色", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleDelete, SortOrder = 204, IsBuiltIn = true },
            new Menu { Name = "查看菜单", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuView, SortOrder = 301, IsBuiltIn = true },
            new Menu { Name = "新建菜单", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuCreate, SortOrder = 302, IsBuiltIn = true },
            new Menu { Name = "编辑菜单", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuUpdate, SortOrder = 303, IsBuiltIn = true },
            new Menu { Name = "删除菜单", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuDelete, SortOrder = 304, IsBuiltIn = true },
            new Menu { Name = "查看部门", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentView, SortOrder = 401, IsBuiltIn = true },
            new Menu { Name = "新建部门", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentCreate, SortOrder = 402, IsBuiltIn = true },
            new Menu { Name = "编辑部门", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentUpdate, SortOrder = 403, IsBuiltIn = true },
            new Menu { Name = "删除部门", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentDelete, SortOrder = 404, IsBuiltIn = true },
            new Menu { Name = "查看岗位", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionView, SortOrder = 501, IsBuiltIn = true },
            new Menu { Name = "新建岗位", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionCreate, SortOrder = 502, IsBuiltIn = true },
            new Menu { Name = "编辑岗位", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionUpdate, SortOrder = 503, IsBuiltIn = true },
            new Menu { Name = "删除岗位", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionDelete, SortOrder = 504, IsBuiltIn = true },
            new Menu { Name = "查看操作日志", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.OperationLogView, SortOrder = 601, IsBuiltIn = true },
            new Menu { Name = "查看登录日志", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.LoginLogView, SortOrder = 602, IsBuiltIn = true },
            new Menu { Name = "查看会话", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.SessionView, SortOrder = 701, IsBuiltIn = true },
            new Menu { Name = "强制下线", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.SessionRevoke, SortOrder = 702, IsBuiltIn = true },
            new Menu { Name = "查看配置", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.SettingView, SortOrder = 801, IsBuiltIn = true },
            new Menu { Name = "编辑配置", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.SettingUpdate, SortOrder = 802, IsBuiltIn = true }
        };

        var system = await EnsureMenuAsync(definitions[0], null, cancellationToken);
        var dashboard = await EnsureMenuAsync(definitions[1], system.Id, cancellationToken);
        var users = await EnsureMenuAsync(definitions[2], system.Id, cancellationToken);
        var roles = await EnsureMenuAsync(definitions[3], system.Id, cancellationToken);
        var departments = await EnsureMenuAsync(definitions[4], system.Id, cancellationToken);
        var positions = await EnsureMenuAsync(definitions[5], system.Id, cancellationToken);
        var menus = await EnsureMenuAsync(definitions[6], system.Id, cancellationToken);
        var logs = await EnsureMenuAsync(definitions[7], system.Id, cancellationToken);
        var sessions = await EnsureMenuAsync(definitions[8], system.Id, cancellationToken);
        var settings = await EnsureMenuAsync(definitions[9], system.Id, cancellationToken);
        var saved = new List<Menu> { system, dashboard, users, roles, departments, positions, menus, logs, sessions, settings };

        foreach (var definition in definitions.Skip(10))
        {
            var parentId = definition.PermissionCode switch
            {
                SystemPermissionCodes.UserView or SystemPermissionCodes.UserCreate or SystemPermissionCodes.UserUpdate or SystemPermissionCodes.UserDelete => users.Id,
                SystemPermissionCodes.RoleView or SystemPermissionCodes.RoleCreate or SystemPermissionCodes.RoleUpdate or SystemPermissionCodes.RoleDelete => roles.Id,
                SystemPermissionCodes.MenuView or SystemPermissionCodes.MenuCreate or SystemPermissionCodes.MenuUpdate or SystemPermissionCodes.MenuDelete => menus.Id,
                SystemPermissionCodes.DepartmentView or SystemPermissionCodes.DepartmentCreate or SystemPermissionCodes.DepartmentUpdate or SystemPermissionCodes.DepartmentDelete => departments.Id,
                SystemPermissionCodes.PositionView or SystemPermissionCodes.PositionCreate or SystemPermissionCodes.PositionUpdate or SystemPermissionCodes.PositionDelete => positions.Id,
                SystemPermissionCodes.OperationLogView or SystemPermissionCodes.LoginLogView => logs.Id,
                SystemPermissionCodes.SessionView or SystemPermissionCodes.SessionRevoke => sessions.Id,
                SystemPermissionCodes.SettingView or SystemPermissionCodes.SettingUpdate => settings.Id,
                _ => system.Id
            };
            saved.Add(await EnsureMenuAsync(definition, parentId, cancellationToken));
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
