using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Application.Menus;
using SunAdmin.Contracts.Menus;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class MenuService(IFreeSql freeSql, IEntityAuditService auditService) : IMenuService
{
    public async Task<IReadOnlyList<MenuTreeNodeDto>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var menus = await freeSql.Select<Menu>()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
        return MenuTreeBuilder.Build(menus, includeButtons: true);
    }

    public async Task<MenuDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var menu = await freeSql.Select<Menu>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken);
        return menu is null ? null : ToDto(menu);
    }

    public async Task<MenuDto> CreateAsync(CreateMenuRequest request, CancellationToken cancellationToken = default)
    {
        var fields = NormalizeFields(request.Type, request.RoutePath, request.Component, request.Icon, request.PermissionCode);
        EnsureTypeRules(request.Type, fields.RoutePath, fields.PermissionCode);
        await EnsureParentExistsAsync(request.ParentId, null, cancellationToken);
        await EnsurePermissionCodeAvailableAsync(fields.PermissionCode, null, cancellationToken);
        var menu = new Menu
        {
            ParentId = request.ParentId,
            Name = request.Name.Trim(),
            Type = request.Type,
            RoutePath = fields.RoutePath,
            Component = fields.Component,
            Icon = fields.Icon,
            PermissionCode = fields.PermissionCode,
            SortOrder = request.SortOrder
        };
        menu.Id = await freeSql.Insert(menu).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Menu), menu.Id.ToString(), "Create", null, menu, cancellationToken);
        return ToDto(menu);
    }

    public async Task<MenuDto> UpdateAsync(long id, UpdateMenuRequest request, CancellationToken cancellationToken = default)
    {
        var menu = await GetEntityAsync(id, cancellationToken);
        var before = Clone(menu);
        if (menu.IsBuiltIn && request.Status == RecordStatus.Disabled)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in menu cannot be disabled.");
        }

        var fields = NormalizeFields(request.Type, request.RoutePath, request.Component, request.Icon, request.PermissionCode);
        EnsureTypeRules(request.Type, fields.RoutePath, fields.PermissionCode);
        await EnsureParentExistsAsync(request.ParentId, id, cancellationToken);
        await EnsurePermissionCodeAvailableAsync(fields.PermissionCode, id, cancellationToken);
        menu.ParentId = request.ParentId;
        menu.Name = request.Name.Trim();
        menu.Type = request.Type;
        menu.RoutePath = fields.RoutePath;
        menu.Component = fields.Component;
        menu.Icon = fields.Icon;
        menu.PermissionCode = fields.PermissionCode;
        menu.SortOrder = request.SortOrder;
        menu.Status = request.Status;
        menu.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<Menu>().SetSource(menu).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Menu), menu.Id.ToString(), "Update", before, menu, cancellationToken);
        return ToDto(menu);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var menu = await GetEntityAsync(id, cancellationToken);
        if (menu.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in menu cannot be deleted.");
        }

        if (await freeSql.Select<Menu>().Where(x => x.ParentId == id && x.DeletedAt == null).AnyAsync(cancellationToken))
        {
            throw new BusinessException("BUSINESS_ERROR", "Menu with children cannot be deleted.");
        }

        menu.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<Menu>().SetSource(menu).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Menu), menu.Id.ToString(), "Delete", menu, null, cancellationToken);
    }

    private async Task EnsureParentExistsAsync(long? parentId, long? currentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        if (currentId.HasValue && parentId.Value == currentId.Value)
        {
            throw new BusinessException("BUSINESS_ERROR", "Menu cannot use itself as parent.");
        }

        var parent = await freeSql.Select<Menu>().Where(x => x.Id == parentId.Value && x.DeletedAt == null).FirstAsync(cancellationToken);
        if (parent is null)
        {
            throw new BusinessException("BUSINESS_ERROR", "Parent menu not found.");
        }

        if (!currentId.HasValue)
        {
            return;
        }

        var menus = await freeSql.Select<Menu>().Where(x => x.DeletedAt == null).ToListAsync(cancellationToken);
        var cursor = parent;
        while (cursor.ParentId.HasValue)
        {
            if (cursor.ParentId.Value == currentId.Value)
            {
                throw new BusinessException("BUSINESS_ERROR", "Menu cannot use a descendant as parent.");
            }

            var next = menus.FirstOrDefault(x => x.Id == cursor.ParentId.Value);
            if (next is null)
            {
                break;
            }

            cursor = next;
        }
    }

    private async Task EnsurePermissionCodeAvailableAsync(string? permissionCode, long? currentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
        {
            return;
        }

        var exists = await freeSql.Select<Menu>()
            .Where(x => x.DeletedAt == null && x.PermissionCode == permissionCode && (!currentId.HasValue || x.Id != currentId.Value))
            .AnyAsync(cancellationToken);
        if (exists)
        {
            throw new BusinessException("CONFLICT", "Permission code already exists.");
        }
    }

    private static void EnsureTypeRules(MenuType type, string? routePath, string? permissionCode)
    {
        if (type == MenuType.Page && string.IsNullOrWhiteSpace(routePath))
        {
            throw new BusinessException("VALIDATION_ERROR", "Page menu routePath is required.");
        }

        if (type == MenuType.Button && string.IsNullOrWhiteSpace(permissionCode))
        {
            throw new BusinessException("VALIDATION_ERROR", "Button menu permissionCode is required.");
        }
    }

    private static MenuFields NormalizeFields(
        MenuType type,
        string? routePath,
        string? component,
        string? icon,
        string? permissionCode)
    {
        return type switch
        {
            MenuType.Directory => new MenuFields(null, null, TrimToNull(icon), null),
            MenuType.Page => new MenuFields(TrimToNull(routePath), TrimToNull(component), TrimToNull(icon), null),
            MenuType.Button => new MenuFields(null, null, TrimToNull(icon), TrimToNull(permissionCode)),
            _ => new MenuFields(TrimToNull(routePath), TrimToNull(component), TrimToNull(icon), TrimToNull(permissionCode))
        };
    }

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private async Task<Menu> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<Menu>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Menu not found.");
    }

    private static MenuDto ToDto(Menu menu)
    {
        return new MenuDto(menu.Id, menu.ParentId, menu.Name, menu.Type, menu.RoutePath, menu.Component, menu.Icon, menu.PermissionCode, menu.SortOrder, menu.Status, menu.IsBuiltIn);
    }

    private static Menu Clone(Menu value)
    {
        return new Menu
        {
            Id = value.Id,
            ParentId = value.ParentId,
            Name = value.Name,
            Type = value.Type,
            RoutePath = value.RoutePath,
            Component = value.Component,
            Icon = value.Icon,
            PermissionCode = value.PermissionCode,
            SortOrder = value.SortOrder,
            Status = value.Status,
            IsBuiltIn = value.IsBuiltIn,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }

    private sealed record MenuFields(string? RoutePath, string? Component, string? Icon, string? PermissionCode);
}
