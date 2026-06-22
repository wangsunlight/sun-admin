using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Menus;

public sealed record MenuDto(
    long Id,
    long? ParentId,
    string Name,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Icon,
    string? PermissionCode,
    int SortOrder,
    RecordStatus Status,
    bool IsBuiltIn);

public sealed record MenuTreeNodeDto(
    long Id,
    long? ParentId,
    string Name,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Icon,
    string? PermissionCode,
    int SortOrder,
    IReadOnlyList<MenuTreeNodeDto> Children);

public sealed record CreateMenuRequest(
    long? ParentId,
    string Name,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Icon,
    string? PermissionCode,
    int SortOrder);

public sealed record UpdateMenuRequest(
    long? ParentId,
    string Name,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Icon,
    string? PermissionCode,
    int SortOrder,
    RecordStatus Status);
