using SunAdmin.Application.Menus;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.UnitTests;

public sealed class MenuTreeBuilderTests
{
    [Fact]
    public void Build_ExcludesButtons_ByDefault()
    {
        var menus = new[]
        {
            new Menu { Id = 1, Name = "System", Type = MenuType.Directory, SortOrder = 1 },
            new Menu { Id = 2, ParentId = 1, Name = "Users", Type = MenuType.Page, SortOrder = 2 },
            new Menu { Id = 3, ParentId = 2, Name = "Create", Type = MenuType.Button, PermissionCode = "user:create", SortOrder = 3 }
        };

        var tree = MenuTreeBuilder.Build(menus);

        Assert.Single(tree);
        Assert.Single(tree[0].Children);
        Assert.Empty(tree[0].Children[0].Children);
    }
}
