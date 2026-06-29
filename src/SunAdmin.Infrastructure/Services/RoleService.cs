using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Roles;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class RoleService(IFreeSql freeSql, IEntityAuditService auditService) : IRoleService
{
    public async Task<PagedResult<RoleDto>> GetPageAsync(RoleQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<Role>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.Code.Contains(query.Keyword) || x.Name.Contains(query.Keyword));
        }

        var total = await selector.CountAsync(cancellationToken);
        var roles = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        var items = new List<RoleDto>();
        foreach (var role in roles)
        {
            items.Add(await ToDtoAsync(role, cancellationToken));
        }

        return new PagedResult<RoleDto>(items, total, pageIndex, pageSize);
    }

    public async Task<RoleDto?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await freeSql.Select<Role>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken);
        return role is null ? null : await ToDtoAsync(role, cancellationToken);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (await freeSql.Select<Role>().Where(x => x.DeletedAt == null && x.Code == request.Code).AnyAsync(cancellationToken))
        {
            throw new BusinessException("CONFLICT", "Role code already exists.");
        }

        var role = new Role { Code = request.Code, Name = request.Name, Description = request.Description, DataScope = request.DataScope };
        role.Id = await freeSql.Insert(role).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Role), role.Id.ToString(), "Create", null, role, cancellationToken);
        return await ToDtoAsync(role, cancellationToken);
    }

    public async Task<RoleDto> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await GetEntityAsync(id, cancellationToken);
        var before = Clone(role);
        if (role.IsBuiltIn && request.Status == SunAdmin.Domain.Enums.RecordStatus.Disabled)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in role cannot be disabled.");
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.DataScope = request.DataScope;
        role.Status = request.Status;
        role.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<Role>().SetSource(role).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Role), role.Id.ToString(), "Update", before, role, cancellationToken);
        return await ToDtoAsync(role, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var role = await GetEntityAsync(id, cancellationToken);
        if (role.IsBuiltIn)
        {
            throw new BusinessException("BUSINESS_ERROR", "Built-in role cannot be deleted.");
        }

        if (await freeSql.Select<UserRole>().Where(x => x.RoleId == id).AnyAsync(cancellationToken))
        {
            throw new BusinessException("BUSINESS_ERROR", "Role is assigned to users.");
        }

        role.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<Role>().SetSource(role).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(Role), role.Id.ToString(), "Delete", role, null, cancellationToken);
    }

    public async Task AssignMenusAsync(long id, AssignRoleMenusRequest request, CancellationToken cancellationToken = default)
    {
        _ = await GetEntityAsync(id, cancellationToken);
        var targetMenuIds = request.MenuIds.Distinct().ToHashSet();
        var current = await freeSql.Select<RoleMenu>().Where(x => x.RoleId == id).ToListAsync(cancellationToken);
        var currentMenuIds = current.Select(x => x.MenuId).ToHashSet();
        var idsToDelete = current.Where(x => !targetMenuIds.Contains(x.MenuId)).Select(x => x.Id).ToList();
        if (idsToDelete.Count > 0)
        {
            await freeSql.Delete<RoleMenu>().Where(x => idsToDelete.Contains(x.Id)).ExecuteAffrowsAsync(cancellationToken);
        }

        var idsToInsert = targetMenuIds.Except(currentMenuIds).ToList();
        if (idsToInsert.Count > 0)
        {
            await freeSql.Insert(idsToInsert.Select(menuId => new RoleMenu { RoleId = id, MenuId = menuId })).ExecuteAffrowsAsync(cancellationToken);
        }

        await auditService.WriteAsync(nameof(RoleMenu), id.ToString(), "AssignMenus", null, new { RoleId = id, MenuIds = targetMenuIds }, cancellationToken);
    }

    private async Task<Role> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<Role>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Role not found.");
    }

    private async Task<RoleDto> ToDtoAsync(Role role, CancellationToken cancellationToken)
    {
        var menuIds = await freeSql.Select<RoleMenu>()
            .Where(x => x.RoleId == role.Id)
            .ToListAsync(x => x.MenuId, cancellationToken);
        var userCount = await freeSql.Select<UserRole>().Where(x => x.RoleId == role.Id).CountAsync(cancellationToken);
        return new RoleDto(role.Id, role.Code, role.Name, role.Description, role.DataScope, role.Status, role.IsBuiltIn, (int)userCount, role.CreatedAt, menuIds);
    }

    private static Role Clone(Role value)
    {
        return new Role
        {
            Id = value.Id,
            Code = value.Code,
            Name = value.Name,
            Description = value.Description,
            DataScope = value.DataScope,
            Status = value.Status,
            IsBuiltIn = value.IsBuiltIn,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }
}
