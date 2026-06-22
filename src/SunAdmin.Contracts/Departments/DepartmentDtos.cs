using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Departments;

public sealed record DepartmentDto(
    long Id,
    long? ParentId,
    string Code,
    string Name,
    string? Leader,
    string? Phone,
    string? Email,
    int SortOrder,
    RecordStatus Status,
    bool IsBuiltIn,
    DateTime CreatedAt,
    IReadOnlyList<DepartmentDto> Children);

public sealed record CreateDepartmentRequest(
    long? ParentId,
    string Code,
    string Name,
    string? Leader,
    string? Phone,
    string? Email,
    int SortOrder);

public sealed record UpdateDepartmentRequest(
    long? ParentId,
    string Code,
    string Name,
    string? Leader,
    string? Phone,
    string? Email,
    int SortOrder,
    RecordStatus Status);
