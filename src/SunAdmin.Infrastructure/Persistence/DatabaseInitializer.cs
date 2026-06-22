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
            "Super Administrator",
            "Built-in role with all permissions.",
            cancellationToken);

        var seed = seedOptions.Value;
        var admin = await freeSql.Select<User>().Where(x => x.UserName == seed.AdminUserName).FirstAsync(cancellationToken);
        if (admin is null)
        {
            admin = new User
            {
                UserName = seed.AdminUserName,
                DisplayName = "Administrator",
                Email = seed.AdminEmail,
                PasswordHash = passwordHasher.HashPassword(seed.AdminPassword),
                IsBuiltIn = true
            };
            admin.Id = await freeSql.Insert(admin).ExecuteIdentityAsync(cancellationToken);
        }

        var menus = await EnsureMenusAsync(cancellationToken);
        await EnsureDepartmentsAsync(cancellationToken);
        await EnsurePositionsAsync(cancellationToken);
        var readonlyRole = await EnsureRoleAsync(
            ReadonlyAdminRoleCode,
            "Readonly Administrator",
            "Example role that can view users, roles, and menus without write permissions.",
            cancellationToken);
        var userAdminRole = await EnsureRoleAsync(
            UserAdminRoleCode,
            "User Administrator",
            "Example role that can manage users but cannot manage roles or menus.",
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
                    x.PermissionCode is SystemPermissionCodes.UserView or SystemPermissionCodes.RoleView or SystemPermissionCodes.MenuView or SystemPermissionCodes.DepartmentView or SystemPermissionCodes.PositionView)
                .Select(x => x.Id),
            cancellationToken);
        await EnsureRoleMenusAsync(
            userAdminRole.Id,
            menus.Where(x =>
                    x.Type is MenuType.Directory ||
                    x.Name is "Dashboard" or "Users" or "Departments" or "Positions" ||
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
            new Menu { Name = "System", Type = MenuType.Directory, SortOrder = 10, IsBuiltIn = true },
            new Menu { Name = "Dashboard", Type = MenuType.Page, RoutePath = "/dashboard", Component = "dashboard/DashboardPage", SortOrder = 11, IsBuiltIn = true },
            new Menu { Name = "Users", Type = MenuType.Page, RoutePath = "/users", Component = "users/UserManagementPage", SortOrder = 12, IsBuiltIn = true },
            new Menu { Name = "Roles", Type = MenuType.Page, RoutePath = "/roles", Component = "roles/RoleManagementPage", SortOrder = 13, IsBuiltIn = true },
            new Menu { Name = "Departments", Type = MenuType.Page, RoutePath = "/departments", Component = "departments/DepartmentManagementPage", SortOrder = 14, IsBuiltIn = true },
            new Menu { Name = "Positions", Type = MenuType.Page, RoutePath = "/positions", Component = "positions/PositionManagementPage", SortOrder = 15, IsBuiltIn = true },
            new Menu { Name = "Menus", Type = MenuType.Page, RoutePath = "/menus", Component = "menus/MenuManagementPage", SortOrder = 16, IsBuiltIn = true },
            new Menu { Name = "View Users", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserView, SortOrder = 101, IsBuiltIn = true },
            new Menu { Name = "Create User", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserCreate, SortOrder = 102, IsBuiltIn = true },
            new Menu { Name = "Update User", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserUpdate, SortOrder = 103, IsBuiltIn = true },
            new Menu { Name = "Delete User", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.UserDelete, SortOrder = 104, IsBuiltIn = true },
            new Menu { Name = "View Roles", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleView, SortOrder = 201, IsBuiltIn = true },
            new Menu { Name = "Create Role", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleCreate, SortOrder = 202, IsBuiltIn = true },
            new Menu { Name = "Update Role", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleUpdate, SortOrder = 203, IsBuiltIn = true },
            new Menu { Name = "Delete Role", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.RoleDelete, SortOrder = 204, IsBuiltIn = true },
            new Menu { Name = "View Menus", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuView, SortOrder = 301, IsBuiltIn = true },
            new Menu { Name = "Create Menu", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuCreate, SortOrder = 302, IsBuiltIn = true },
            new Menu { Name = "Update Menu", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuUpdate, SortOrder = 303, IsBuiltIn = true },
            new Menu { Name = "Delete Menu", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.MenuDelete, SortOrder = 304, IsBuiltIn = true },
            new Menu { Name = "View Departments", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentView, SortOrder = 401, IsBuiltIn = true },
            new Menu { Name = "Create Department", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentCreate, SortOrder = 402, IsBuiltIn = true },
            new Menu { Name = "Update Department", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentUpdate, SortOrder = 403, IsBuiltIn = true },
            new Menu { Name = "Delete Department", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.DepartmentDelete, SortOrder = 404, IsBuiltIn = true },
            new Menu { Name = "View Positions", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionView, SortOrder = 501, IsBuiltIn = true },
            new Menu { Name = "Create Position", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionCreate, SortOrder = 502, IsBuiltIn = true },
            new Menu { Name = "Update Position", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionUpdate, SortOrder = 503, IsBuiltIn = true },
            new Menu { Name = "Delete Position", Type = MenuType.Button, PermissionCode = SystemPermissionCodes.PositionDelete, SortOrder = 504, IsBuiltIn = true }
        };

        var system = await EnsureMenuAsync(definitions[0], null, cancellationToken);
        var dashboard = await EnsureMenuAsync(definitions[1], system.Id, cancellationToken);
        var users = await EnsureMenuAsync(definitions[2], system.Id, cancellationToken);
        var roles = await EnsureMenuAsync(definitions[3], system.Id, cancellationToken);
        var departments = await EnsureMenuAsync(definitions[4], system.Id, cancellationToken);
        var positions = await EnsureMenuAsync(definitions[5], system.Id, cancellationToken);
        var menus = await EnsureMenuAsync(definitions[6], system.Id, cancellationToken);
        var saved = new List<Menu> { system, dashboard, users, roles, departments, positions, menus };

        foreach (var definition in definitions.Skip(7))
        {
            var parentId = definition.PermissionCode switch
            {
                SystemPermissionCodes.UserView or SystemPermissionCodes.UserCreate or SystemPermissionCodes.UserUpdate or SystemPermissionCodes.UserDelete => users.Id,
                SystemPermissionCodes.RoleView or SystemPermissionCodes.RoleCreate or SystemPermissionCodes.RoleUpdate or SystemPermissionCodes.RoleDelete => roles.Id,
                SystemPermissionCodes.MenuView or SystemPermissionCodes.MenuCreate or SystemPermissionCodes.MenuUpdate or SystemPermissionCodes.MenuDelete => menus.Id,
                SystemPermissionCodes.DepartmentView or SystemPermissionCodes.DepartmentCreate or SystemPermissionCodes.DepartmentUpdate or SystemPermissionCodes.DepartmentDelete => departments.Id,
                SystemPermissionCodes.PositionView or SystemPermissionCodes.PositionCreate or SystemPermissionCodes.PositionUpdate or SystemPermissionCodes.PositionDelete => positions.Id,
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
            Leader = "Administrator",
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

    private async Task<Menu> EnsureMenuAsync(Menu definition, long? parentId, CancellationToken cancellationToken)
    {
        var existing = !string.IsNullOrWhiteSpace(definition.PermissionCode)
            ? await freeSql.Select<Menu>().Where(x => x.PermissionCode == definition.PermissionCode).FirstAsync(cancellationToken)
            : await freeSql.Select<Menu>().Where(x => x.Name == definition.Name && x.Type == definition.Type).FirstAsync(cancellationToken);

        if (existing is not null)
        {
            existing.ParentId = parentId;
            existing.RoutePath = definition.RoutePath;
            existing.Component = definition.Component;
            existing.SortOrder = definition.SortOrder;
            existing.IsBuiltIn = definition.IsBuiltIn;
            existing.UpdatedAt = DateTime.UtcNow;
            await freeSql.Update<Menu>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
            return existing;
        }

        definition.ParentId = parentId;
        definition.Id = await freeSql.Insert(definition).ExecuteIdentityAsync(cancellationToken);
        return definition;
    }
}
