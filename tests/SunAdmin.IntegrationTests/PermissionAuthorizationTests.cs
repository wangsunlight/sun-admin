using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Menus;
using SunAdmin.Contracts.Roles;
using SunAdmin.Contracts.Users;
using SunAdmin.Domain.Constants;
using SunAdmin.Domain.Enums;

namespace SunAdmin.IntegrationTests;

public sealed class PermissionAuthorizationTests
{
    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/roles")]
    [InlineData("/api/menus/tree")]
    public async Task ProtectedManagementApis_WithoutLogin_ReturnUnauthorized(string path)
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/roles")]
    [InlineData("/api/menus/tree")]
    public async Task ProtectedManagementApis_WithoutPermission_ReturnForbidden(string path)
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "3");

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/roles")]
    [InlineData("/api/menus/tree")]
    public async Task ProtectedManagementApis_WithRequiredPermission_ReturnSuccess(string path)
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "2");

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_CanAccessUserRoleAndMenuWriteApis_WithoutExplicitPermissionGrant()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, "1");

        var userResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
            "new-user",
            "New User",
            "new-user@sun-admin.local",
            null,
            null,
            "Admin@123456",
            []));
        var roleResponse = await client.PostAsJsonAsync("/api/roles", new CreateRoleRequest(
            "example_role",
            "Example Role",
            "Created by authorization test."));
        var menuResponse = await client.PostAsJsonAsync("/api/menus", new CreateMenuRequest(
            null,
            "Example",
            MenuType.Page,
            "/example",
            "example/ExamplePage",
            null,
            null,
            99));

        Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, menuResponse.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:DisableInitializer"] = "true"
                    });
                });
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                        options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                        options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

                    services.RemoveAll<IPermissionChecker>();
                    services.RemoveAll<IUserService>();
                    services.RemoveAll<IRoleService>();
                    services.RemoveAll<IMenuService>();
                    services.AddSingleton<IPermissionChecker, TestPermissionChecker>();
                    services.AddSingleton<IUserService, TestUserService>();
                    services.AddSingleton<IRoleService, TestRoleService>();
                    services.AddSingleton<IMenuService, TestMenuService>();
                });
            });
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";
        public const string UserIdHeader = "X-Test-UserId";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserIdHeader, out var values) || !long.TryParse(values.FirstOrDefault(), out var userId))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, $"test-user-{userId}")
            };
            if (userId == 1)
            {
                claims.Add(new Claim(ClaimTypes.Role, SystemRoleCodes.SuperAdmin));
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
        }
    }

    private sealed class TestPermissionChecker : IPermissionChecker
    {
        private static readonly IReadOnlySet<string> UserTwoPermissions = new HashSet<string>
        {
            SystemPermissionCodes.UserView,
            SystemPermissionCodes.RoleView,
            SystemPermissionCodes.MenuView
        };

        public Task<bool> IsSuperAdminAsync(long userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(userId == 1);
        }

        public async Task<bool> HasPermissionAsync(long userId, string permissionCode, CancellationToken cancellationToken = default)
        {
            return await IsSuperAdminAsync(userId, cancellationToken) || (userId == 2 && UserTwoPermissions.Contains(permissionCode));
        }
    }

    private sealed class TestUserService : IUserService
    {
        private static readonly UserDto User = new(2, "reader", "Reader", "reader@sun-admin.local", null, null, null, null, RecordStatus.Enabled, false, false, DateTime.UtcNow, null, []);

        public Task<PagedResult<UserDto>> GetPageAsync(UserQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<UserDto>([User], 1, query.PageIndex, query.PageSize));
        }

        public Task<UserDto?> GetAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<UserDto?>(User);

        public Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(User with
            {
                UserName = request.UserName,
                DisplayName = request.DisplayName,
                Email = request.Email
            });
        }

        public Task<UserDto> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default) => Task.FromResult(User);

        public Task DeleteAsync(long id, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SetEnabledAsync(long id, bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ResetPasswordAsync(long id, ResetPasswordRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AssignRolesAsync(long id, AssignUserRolesRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task BatchEnableAsync(BatchUserRequest request, bool enabled, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task BatchDeleteAsync(BatchUserRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestRoleService : IRoleService
    {
        private static readonly RoleDto Role = new(1, "readonly_admin", "Readonly Administrator", null, RoleDataScope.All, RecordStatus.Enabled, true, 0, DateTime.UtcNow, []);

        public Task<PagedResult<RoleDto>> GetPageAsync(RoleQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PagedResult<RoleDto>([Role], 1, query.PageIndex, query.PageSize));
        }

        public Task<RoleDto?> GetAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<RoleDto?>(Role);

        public Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Role with
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description
            });
        }

        public Task<RoleDto> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Role);

        public Task DeleteAsync(long id, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AssignMenusAsync(long id, AssignRoleMenusRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestMenuService : IMenuService
    {
        private static readonly MenuDto Menu = new(1, null, "Dashboard", MenuType.Page, "/dashboard", "dashboard/DashboardPage", null, null, 1, RecordStatus.Enabled, true);

        public Task<IReadOnlyList<MenuTreeNodeDto>> GetTreeAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<MenuTreeNodeDto> menus =
            [
                new(Menu.Id, Menu.ParentId, Menu.Name, Menu.Type, Menu.RoutePath, Menu.Component, Menu.Icon, Menu.PermissionCode, Menu.SortOrder, [])
            ];
            return Task.FromResult(menus);
        }

        public Task<MenuDto?> GetAsync(long id, CancellationToken cancellationToken = default) => Task.FromResult<MenuDto?>(Menu);

        public Task<MenuDto> CreateAsync(CreateMenuRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Menu with
            {
                Name = request.Name,
                Type = request.Type,
                RoutePath = request.RoutePath,
                Component = request.Component,
                PermissionCode = request.PermissionCode,
                SortOrder = request.SortOrder
            });
        }

        public Task<MenuDto> UpdateAsync(long id, UpdateMenuRequest request, CancellationToken cancellationToken = default) => Task.FromResult(Menu);

        public Task DeleteAsync(long id, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
