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

        return services;
    }

    private static void ConfigureEntities(IFreeSql freeSql)
    {
        freeSql.CodeFirst.ConfigEntity<User>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<Role>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<Menu>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<Department>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<Position>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<OperationLog>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<LoginLog>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<LoginSession>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<SystemSetting>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<UserRole>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
        freeSql.CodeFirst.ConfigEntity<RoleMenu>(entity => entity.Property(x => x.Id).IsPrimary(true).IsIdentity(true));
    }
}
