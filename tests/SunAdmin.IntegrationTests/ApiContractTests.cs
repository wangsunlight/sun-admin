using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SunAdmin.Contracts.Roles;
using SunAdmin.Domain.Enums;

namespace SunAdmin.IntegrationTests;

public sealed class ApiContractTests
{
    [Fact]
    public void MvcJsonOptions_SerializesEnumsAsStrings_AndRoleMenuIds()
    {
        using var factory = CreateFactory();
        var jsonOptions = factory.Services.GetRequiredService<IOptions<JsonOptions>>().Value;
        var role = new RoleDto(
            Id: 7,
            Code: "admin",
            Name: "Administrator",
            Description: null,
            Status: RecordStatus.Enabled,
            IsBuiltIn: true,
            CreatedAt: new DateTime(2026, 6, 22, 8, 0, 0, DateTimeKind.Utc),
            MenuIds: [1, 2, 3]);

        var json = JsonSerializer.Serialize(role, jsonOptions.JsonSerializerOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Enabled", root.GetProperty("status").GetString());
        Assert.Equal([1L, 2L, 3L], root.GetProperty("menuIds").EnumerateArray().Select(x => x.GetInt64()).ToArray());
    }

    [Fact]
    public async Task AuthMe_WithoutBearerToken_ReturnsUnauthorized()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
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
            });
    }
}
