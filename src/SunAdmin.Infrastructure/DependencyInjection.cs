using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SunAdmin.Application.Abstractions;
using SunAdmin.Domain.Entities;
using SunAdmin.Infrastructure.Options;
using SunAdmin.Infrastructure.Persistence;
using SunAdmin.Infrastructure.Security;
using SunAdmin.Infrastructure.Services;

namespace SunAdmin.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.Configure<SeedOptions>(configuration.GetSection("Seed"));

        services.AddSingleton(provider =>
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? "Server=localhost;Port=3306;Database=sun_admin;Uid=root;Pwd=root;Charset=utf8mb4;";
            var freeSql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.MySql, connectionString)
                .UseAutoSyncStructure(false)
                .Build();

            ConfigureEntities(freeSql);
            return freeSql;
        });

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<ILogQueryService, LogQueryService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
        services.AddScoped<IEntityAuditService, EntityAuditService>();
        services.AddScoped<IDictionaryService, DictionaryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileResourceService, FileResourceService>();
        services.AddScoped<IExportTaskService, ExportTaskService>();
        services.AddScoped<ICodeGenerationService, CodeGenerationService>();

        return services;
    }

    private static void ConfigureEntities(IFreeSql freeSql)
    {
        freeSql.CodeFirst.ConfigEntity<User>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_User_UserName", nameof(User.UserName), true);
            entity.Index("UX_User_Email", nameof(User.Email), true);
            entity.Index("IX_User_Status", nameof(User.Status), false);
            entity.Index("IX_User_DepartmentId", nameof(User.DepartmentId), false);
        });
        freeSql.CodeFirst.ConfigEntity<Role>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_Role_Code", nameof(Role.Code), true);
            entity.Index("IX_Role_Status", nameof(Role.Status), false);
        });
        freeSql.CodeFirst.ConfigEntity<Menu>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_Menu_PermissionCode", nameof(Menu.PermissionCode), true);
            entity.Index("IX_Menu_ParentId", nameof(Menu.ParentId), false);
            entity.Index("IX_Menu_RoutePath", nameof(Menu.RoutePath), false);
        });
        freeSql.CodeFirst.ConfigEntity<Department>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_Department_Code", nameof(Department.Code), true);
            entity.Index("IX_Department_ParentId", nameof(Department.ParentId), false);
        });
        freeSql.CodeFirst.ConfigEntity<Position>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_Position_Code", nameof(Position.Code), true);
        });
        freeSql.CodeFirst.ConfigEntity<OperationLog>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("IX_OperationLog_CreatedAt", nameof(OperationLog.CreatedAt), false);
            entity.Index("IX_OperationLog_Path", nameof(OperationLog.Path), false);
        });
        freeSql.CodeFirst.ConfigEntity<LoginLog>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("IX_LoginLog_CreatedAt", nameof(LoginLog.CreatedAt), false);
            entity.Index("IX_LoginLog_UserId", nameof(LoginLog.UserId), false);
        });
        freeSql.CodeFirst.ConfigEntity<LoginSession>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_LoginSession_SessionId", nameof(LoginSession.SessionId), true);
            entity.Index("IX_LoginSession_UserId", nameof(LoginSession.UserId), false);
            entity.Index("IX_LoginSession_ExpiresAt", nameof(LoginSession.ExpiresAt), false);
        });
        freeSql.CodeFirst.ConfigEntity<SystemSetting>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_SystemSetting_Key", nameof(SystemSetting.Key), true);
        });
        freeSql.CodeFirst.ConfigEntity<UserRole>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_UserRole_UserId_RoleId", $"{nameof(UserRole.UserId)}, {nameof(UserRole.RoleId)}", true);
        });
        freeSql.CodeFirst.ConfigEntity<RoleMenu>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_RoleMenu_RoleId_MenuId", $"{nameof(RoleMenu.RoleId)}, {nameof(RoleMenu.MenuId)}", true);
        });
        freeSql.CodeFirst.ConfigEntity<DataDictionary>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_DataDictionary_Code", nameof(DataDictionary.Code), true);
        });
        freeSql.CodeFirst.ConfigEntity<DataDictionaryItem>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_DataDictionaryItem_DictionaryId_Value", $"{nameof(DataDictionaryItem.DictionaryId)}, {nameof(DataDictionaryItem.Value)}", true);
        });
        freeSql.CodeFirst.ConfigEntity<SystemNotification>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("IX_SystemNotification_PublishAt", nameof(SystemNotification.PublishAt), false);
        });
        freeSql.CodeFirst.ConfigEntity<FileResource>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("IX_FileResource_UploadedBy", nameof(FileResource.UploadedBy), false);
        });
        freeSql.CodeFirst.ConfigEntity<ExportTask>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("IX_ExportTask_Status", nameof(ExportTask.Status), false);
            entity.Index("IX_ExportTask_CreatedByUserId", nameof(ExportTask.CreatedByUserId), false);
        });
        freeSql.CodeFirst.ConfigEntity<CodeGenerationTemplate>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("UX_CodeGenerationTemplate_TemplateKey", nameof(CodeGenerationTemplate.TemplateKey), true);
        });
        freeSql.CodeFirst.ConfigEntity<EntityChangeLog>(entity =>
        {
            entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true);
            entity.Index("IX_EntityChangeLog_Entity", $"{nameof(EntityChangeLog.EntityName)}, {nameof(EntityChangeLog.EntityId)}", false);
            entity.Index("IX_EntityChangeLog_CreatedAt", nameof(EntityChangeLog.CreatedAt), false);
        });
    }
}
