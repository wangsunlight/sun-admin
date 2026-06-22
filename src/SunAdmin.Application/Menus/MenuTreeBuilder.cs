using SunAdmin.Contracts.Menus;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Application.Menus;

public static class MenuTreeBuilder
{
    public static IReadOnlyList<MenuTreeNodeDto> Build(IEnumerable<Menu> menus, bool includeButtons = false)
    {
        var filtered = menus
            .Where(menu => includeButtons || menu.Type is MenuType.Directory or MenuType.Page)
            .OrderBy(menu => menu.SortOrder)
            .ThenBy(menu => menu.Id)
            .ToList();

        return BuildChildren(filtered, null);
    }

    private static IReadOnlyList<MenuTreeNodeDto> BuildChildren(IReadOnlyList<Menu> menus, long? parentId)
    {
        return menus
            .Where(menu => menu.ParentId == parentId)
            .Select(menu => new MenuTreeNodeDto(
                menu.Id,
                menu.ParentId,
                menu.Name,
                menu.Type,
                menu.RoutePath,
                menu.Component,
                menu.Icon,
                menu.PermissionCode,
                menu.SortOrder,
                BuildChildren(menus, menu.Id)))
            .ToList();
    }
}
