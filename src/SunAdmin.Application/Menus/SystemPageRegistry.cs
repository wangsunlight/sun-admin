using SunAdmin.Domain.Constants;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Application.Menus;

public sealed record SystemPageDescriptor(
    string Code,
    string Name,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Icon,
    string? ParentCode,
    string? PermissionCode,
    int SortOrder);

public static class SystemPageRegistry
{
    public static IReadOnlyList<SystemPageDescriptor> Pages { get; } =
    [
        new("system", "系统管理", MenuType.Directory, null, null, null, null, null, 10),
        new("dashboard", "工作台", MenuType.Page, "/dashboard", "dashboard/DashboardPage", null, "system", null, 11),
        new("users", "用户管理", MenuType.Page, "/users", "users/UserManagementPage", null, "system", SystemPermissionCodes.UserView, 12),
        new("roles", "角色管理", MenuType.Page, "/roles", "roles/RoleManagementPage", null, "system", SystemPermissionCodes.RoleView, 13),
        new("departments", "部门管理", MenuType.Page, "/departments", "departments/DepartmentManagementPage", null, "system", SystemPermissionCodes.DepartmentView, 14),
        new("positions", "岗位管理", MenuType.Page, "/positions", "positions/PositionManagementPage", null, "system", SystemPermissionCodes.PositionView, 15),
        new("menus", "菜单管理", MenuType.Page, "/menus", "menus/MenuManagementPage", null, "system", SystemPermissionCodes.MenuView, 16),
        new("logs", "日志审计", MenuType.Page, "/logs", "logs/LogManagementPage", null, "system", SystemPermissionCodes.OperationLogView, 17),
        new("sessions", "在线会话", MenuType.Page, "/sessions", "sessions/SessionManagementPage", null, "system", SystemPermissionCodes.SessionView, 18),
        new("settings", "系统配置", MenuType.Page, "/settings", "settings/SettingManagementPage", null, "system", SystemPermissionCodes.SettingView, 19),
        new("dictionaries", "数据字典", MenuType.Page, "/dictionaries", "platform/DictionaryManagementPage", null, "system", SystemPermissionCodes.DictionaryView, 20),
        new("notifications", "通知公告", MenuType.Page, "/notifications", "platform/NotificationManagementPage", null, "system", SystemPermissionCodes.NotificationView, 21),
        new("files", "文件资源", MenuType.Page, "/files", "platform/FileResourcePage", null, "system", SystemPermissionCodes.FileView, 22),
        new("exports", "导出中心", MenuType.Page, "/exports", "platform/ExportTaskPage", null, "system", SystemPermissionCodes.ExportView, 23),
        new("code-generation", "代码生成", MenuType.Page, "/code-generation", "platform/CodeGenerationPage", null, "system", SystemPermissionCodes.CodeGenerationView, 24),
        new("entity-change-logs", "变更审计", MenuType.Page, "/entity-change-logs", "platform/EntityChangeLogPage", null, "system", SystemPermissionCodes.EntityChangeLogView, 25)
    ];

    public static IReadOnlyList<SystemPageDescriptor> Buttons { get; } =
    [
        Button("users.view", "查看用户", "users", SystemPermissionCodes.UserView, 101),
        Button("users.create", "新建用户", "users", SystemPermissionCodes.UserCreate, 102),
        Button("users.update", "编辑用户", "users", SystemPermissionCodes.UserUpdate, 103),
        Button("users.delete", "删除用户", "users", SystemPermissionCodes.UserDelete, 104),
        Button("roles.view", "查看角色", "roles", SystemPermissionCodes.RoleView, 201),
        Button("roles.create", "新建角色", "roles", SystemPermissionCodes.RoleCreate, 202),
        Button("roles.update", "编辑角色", "roles", SystemPermissionCodes.RoleUpdate, 203),
        Button("roles.delete", "删除角色", "roles", SystemPermissionCodes.RoleDelete, 204),
        Button("menus.view", "查看菜单", "menus", SystemPermissionCodes.MenuView, 301),
        Button("menus.create", "新建菜单", "menus", SystemPermissionCodes.MenuCreate, 302),
        Button("menus.update", "编辑菜单", "menus", SystemPermissionCodes.MenuUpdate, 303),
        Button("menus.delete", "删除菜单", "menus", SystemPermissionCodes.MenuDelete, 304),
        Button("departments.view", "查看部门", "departments", SystemPermissionCodes.DepartmentView, 401),
        Button("departments.create", "新建部门", "departments", SystemPermissionCodes.DepartmentCreate, 402),
        Button("departments.update", "编辑部门", "departments", SystemPermissionCodes.DepartmentUpdate, 403),
        Button("departments.delete", "删除部门", "departments", SystemPermissionCodes.DepartmentDelete, 404),
        Button("positions.view", "查看岗位", "positions", SystemPermissionCodes.PositionView, 501),
        Button("positions.create", "新建岗位", "positions", SystemPermissionCodes.PositionCreate, 502),
        Button("positions.update", "编辑岗位", "positions", SystemPermissionCodes.PositionUpdate, 503),
        Button("positions.delete", "删除岗位", "positions", SystemPermissionCodes.PositionDelete, 504),
        Button("logs.operation", "查看操作日志", "logs", SystemPermissionCodes.OperationLogView, 601),
        Button("logs.login", "查看登录日志", "logs", SystemPermissionCodes.LoginLogView, 602),
        Button("sessions.view", "查看会话", "sessions", SystemPermissionCodes.SessionView, 701),
        Button("sessions.revoke", "强制下线", "sessions", SystemPermissionCodes.SessionRevoke, 702),
        Button("settings.view", "查看配置", "settings", SystemPermissionCodes.SettingView, 801),
        Button("settings.update", "编辑配置", "settings", SystemPermissionCodes.SettingUpdate, 802),
        Button("dictionaries.view", "查看字典", "dictionaries", SystemPermissionCodes.DictionaryView, 901),
        Button("dictionaries.create", "新建字典", "dictionaries", SystemPermissionCodes.DictionaryCreate, 902),
        Button("dictionaries.update", "编辑字典", "dictionaries", SystemPermissionCodes.DictionaryUpdate, 903),
        Button("dictionaries.delete", "删除字典", "dictionaries", SystemPermissionCodes.DictionaryDelete, 904),
        Button("notifications.view", "查看通知", "notifications", SystemPermissionCodes.NotificationView, 1001),
        Button("notifications.create", "新建通知", "notifications", SystemPermissionCodes.NotificationCreate, 1002),
        Button("notifications.update", "编辑通知", "notifications", SystemPermissionCodes.NotificationUpdate, 1003),
        Button("notifications.delete", "删除通知", "notifications", SystemPermissionCodes.NotificationDelete, 1004),
        Button("files.view", "查看文件", "files", SystemPermissionCodes.FileView, 1101),
        Button("files.create", "登记文件", "files", SystemPermissionCodes.FileCreate, 1102),
        Button("files.delete", "删除文件", "files", SystemPermissionCodes.FileDelete, 1103),
        Button("exports.view", "查看导出", "exports", SystemPermissionCodes.ExportView, 1201),
        Button("exports.create", "创建导出", "exports", SystemPermissionCodes.ExportCreate, 1202),
        Button("code-generation.view", "查看模板", "code-generation", SystemPermissionCodes.CodeGenerationView, 1301),
        Button("code-generation.create", "新建模板", "code-generation", SystemPermissionCodes.CodeGenerationCreate, 1302),
        Button("code-generation.update", "编辑模板", "code-generation", SystemPermissionCodes.CodeGenerationUpdate, 1303),
        Button("code-generation.delete", "删除模板", "code-generation", SystemPermissionCodes.CodeGenerationDelete, 1304),
        Button("entity-change-logs.view", "查看变更审计", "entity-change-logs", SystemPermissionCodes.EntityChangeLogView, 1401)
    ];

    public static IReadOnlyList<SystemPageDescriptor> All => Pages.Concat(Buttons).ToList();

    private static SystemPageDescriptor Button(string code, string name, string parentCode, string permissionCode, int sortOrder)
    {
        return new SystemPageDescriptor(code, name, MenuType.Button, null, null, null, parentCode, permissionCode, sortOrder);
    }
}
