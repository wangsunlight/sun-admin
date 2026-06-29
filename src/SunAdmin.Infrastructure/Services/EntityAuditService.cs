using System.Text.Json;
using SunAdmin.Application.Abstractions;
using SunAdmin.Contracts.Audit;
using SunAdmin.Contracts.Common;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class EntityAuditService(IFreeSql freeSql, ICurrentUser currentUser, IRequestContext requestContext) : IEntityAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task WriteAsync(string entityName, string entityId, string changeType, object? before, object? after, CancellationToken cancellationToken = default)
    {
        await freeSql.Insert(new EntityChangeLog
        {
            EntityName = entityName,
            EntityId = entityId,
            ChangeType = changeType,
            ChangedBy = currentUser.UserId,
            ChangedByName = currentUser.UserName,
            ChangedFields = ResolveChangedFields(before, after),
            BeforeJson = before is null ? null : JsonSerializer.Serialize(Sanitize(before), JsonOptions),
            AfterJson = after is null ? null : JsonSerializer.Serialize(Sanitize(after), JsonOptions),
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent
        }).ExecuteAffrowsAsync(cancellationToken);
    }

    public async Task<PagedResult<EntityChangeLogDto>> GetPageAsync(EntityChangeLogQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<EntityChangeLog>();
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.EntityName.Contains(query.Keyword) || x.EntityId.Contains(query.Keyword) || x.ChangedByName!.Contains(query.Keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            selector = selector.Where(x => x.EntityName == query.EntityName);
        }

        if (query.CreatedFrom.HasValue)
        {
            selector = selector.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            selector = selector.Where(x => x.CreatedAt <= query.CreatedTo.Value);
        }

        var total = await selector.CountAsync(cancellationToken);
        var logs = await selector.OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<EntityChangeLogDto>(logs.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    private static string? ResolveChangedFields(object? before, object? after)
    {
        if (before is null || after is null)
        {
            return null;
        }

        var beforeProperties = before.GetType().GetProperties().Where(x => x.GetIndexParameters().Length == 0).ToDictionary(x => x.Name);
        var changed = new List<string>();
        foreach (var afterProperty in after.GetType().GetProperties().Where(x => x.GetIndexParameters().Length == 0))
        {
            if (!beforeProperties.TryGetValue(afterProperty.Name, out var beforeProperty))
            {
                continue;
            }

            if (!Equals(beforeProperty.GetValue(before), afterProperty.GetValue(after)))
            {
                changed.Add(afterProperty.Name);
            }
        }

        return changed.Count == 0 ? null : string.Join(",", changed);
    }

    private static object Sanitize(object value)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in value.GetType().GetProperties().Where(x => x.GetIndexParameters().Length == 0))
        {
            var propertyValue = property.GetValue(value);
            result[property.Name] = IsSensitiveName(property.Name) ? "***" : propertyValue;
        }

        return result;
    }

    private static bool IsSensitiveName(string name)
    {
        return name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("key", StringComparison.OrdinalIgnoreCase) && !name.Equals("Key", StringComparison.OrdinalIgnoreCase);
    }

    private static EntityChangeLogDto ToDto(EntityChangeLog log)
    {
        return new EntityChangeLogDto(
            log.Id,
            log.EntityName,
            log.EntityId,
            log.ChangeType,
            log.ChangedBy,
            log.ChangedByName,
            log.ChangedFields,
            log.BeforeJson,
            log.AfterJson,
            log.IpAddress,
            log.UserAgent,
            log.CreatedAt);
    }
}
