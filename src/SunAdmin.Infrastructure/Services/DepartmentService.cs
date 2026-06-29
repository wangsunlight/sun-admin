using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Departments;
using SunAdmin.Domain.Entities;
using SunAdmin.Domain.Enums;

namespace SunAdmin.Infrastructure.Services;

public sealed class DepartmentService(IFreeSql freeSql, IEntityAuditService auditService) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentDto>> GetTreeAsync(CancellationToken cancellationToken = default)
    {
        var departments = await freeSql.Select<Department>()
            .Where(x => x.DeletedAt == null)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);
        return BuildTree(departments, null);
    }

    public async Task<DepartmentDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var department = await freeSql.Select<Department>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken);
        return department is null ? null : ToDto(department, []);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCodeAvailableAsync(request.Code, null, cancellationToken);
        await EnsureParentExistsAsync(request.ParentId, cancellationToken);
        var department = new Department
        {
            ParentId = request.ParentId,
            Code = request.Code,
            Name = request.Name,
            Leader = request.Leader,
            Phone = request.Phone,
            Email = request.Email,
            SortOrder = request.SortOrder
        };
        department.Id = await freeSql.Insert(department).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Department), department.Id.ToString(), "Create", null, department, cancellationToken);
        return ToDto(department, []);
    }

    public async Task<DepartmentDto> UpdateAsync(long id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var department = await GetEntityAsync(id, cancellationToken);
        var before = Clone(department);
        if (department.IsBuiltIn && request.Status == RecordStatus.Disabled)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in department cannot be disabled.");
        }

        if (request.ParentId == id)
        {
            throw new BusinessException("BUSINESS_ERROR", "Department cannot use itself as parent.");
        }

        await EnsureCodeAvailableAsync(request.Code, id, cancellationToken);
        await EnsureParentExistsAsync(request.ParentId, cancellationToken);
        await EnsureParentIsNotDescendantAsync(id, request.ParentId, cancellationToken);
        department.ParentId = request.ParentId;
        department.Code = request.Code;
        department.Name = request.Name;
        department.Leader = request.Leader;
        department.Phone = request.Phone;
        department.Email = request.Email;
        department.SortOrder = request.SortOrder;
        department.Status = request.Status;
        department.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<Department>().SetSource(department).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Department), department.Id.ToString(), "Update", before, department, cancellationToken);
        return ToDto(department, []);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var department = await GetEntityAsync(id, cancellationToken);
        if (department.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in department cannot be deleted.");
        }

        if (await freeSql.Select<Department>().Where(x => x.ParentId == id && x.DeletedAt == null).AnyAsync(cancellationToken))
        {
            throw new BusinessException("BUSINESS_ERROR", "Department with children cannot be deleted.");
        }

        department.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<Department>().SetSource(department).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Department), department.Id.ToString(), "Delete", department, null, cancellationToken);
    }

    private async Task<Department> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<Department>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Department not found.");
    }

    private async Task EnsureCodeAvailableAsync(string code, long? currentId, CancellationToken cancellationToken)
    {
        var exists = await freeSql.Select<Department>()
            .Where(x => x.DeletedAt == null && x.Code == code && (!currentId.HasValue || x.Id != currentId.Value))
            .AnyAsync(cancellationToken);
        if (exists)
        {
            throw new BusinessException("CONFLICT", "Department code already exists.");
        }
    }

    private async Task EnsureParentExistsAsync(long? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        if (!await freeSql.Select<Department>().Where(x => x.Id == parentId.Value && x.DeletedAt == null).AnyAsync(cancellationToken))
        {
            throw new BusinessException("BUSINESS_ERROR", "Parent department not found.");
        }
    }

    private async Task EnsureParentIsNotDescendantAsync(long currentId, long? parentId, CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        var departments = await freeSql.Select<Department>().Where(x => x.DeletedAt == null).ToListAsync(cancellationToken);
        var cursor = departments.FirstOrDefault(x => x.Id == parentId.Value);
        while (cursor?.ParentId.HasValue == true)
        {
            if (cursor.ParentId.Value == currentId)
            {
                throw new BusinessException("BUSINESS_ERROR", "Department cannot use a descendant as parent.");
            }

            cursor = departments.FirstOrDefault(x => x.Id == cursor.ParentId.Value);
        }
    }

    private static IReadOnlyList<DepartmentDto> BuildTree(IReadOnlyList<Department> departments, long? parentId)
    {
        return departments
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => ToDto(x, BuildTree(departments, x.Id)))
            .ToList();
    }

    private static DepartmentDto ToDto(Department department, IReadOnlyList<DepartmentDto> children)
    {
        return new DepartmentDto(
            department.Id,
            department.ParentId,
            department.Code,
            department.Name,
            department.Leader,
            department.Phone,
            department.Email,
            department.SortOrder,
            department.Status,
            department.IsBuiltIn,
            department.CreatedAt,
            children);
    }

    private static Department Clone(Department value)
    {
        return new Department
        {
            Id = value.Id,
            ParentId = value.ParentId,
            Code = value.Code,
            Name = value.Name,
            Leader = value.Leader,
            Phone = value.Phone,
            Email = value.Email,
            SortOrder = value.SortOrder,
            Status = value.Status,
            IsBuiltIn = value.IsBuiltIn,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }
}
