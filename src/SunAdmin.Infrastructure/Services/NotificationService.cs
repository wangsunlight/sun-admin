using SunAdmin.Application.Abstractions;
using SunAdmin.Application.Common;
using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Notifications;
using SunAdmin.Domain.Entities;

namespace SunAdmin.Infrastructure.Services;

public sealed class NotificationService(IFreeSql freeSql, IEntityAuditService auditService) : INotificationService
{
    public async Task<PagedResult<NotificationDto>> GetPageAsync(NotificationQuery query, CancellationToken cancellationToken = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var selector = freeSql.Select<SystemNotification>().Where(x => x.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            selector = selector.Where(x => x.Title.Contains(query.Keyword) || x.Content.Contains(query.Keyword));
        }

        if (query.Status.HasValue)
        {
            selector = selector.Where(x => x.Status == query.Status.Value);
        }

        var total = await selector.CountAsync(cancellationToken);
        var notifications = await selector.OrderByDescending(x => x.IsPinned).OrderByDescending(x => x.Id).Page(pageIndex, pageSize).ToListAsync(cancellationToken);
        return new PagedResult<NotificationDto>(notifications.Select(ToDto).ToList(), total, pageIndex, pageSize);
    }

    public async Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new SystemNotification
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Level = request.Level,
            PublishAt = request.PublishAt,
            ExpiresAt = request.ExpiresAt,
            IsPinned = request.IsPinned
        };
        notification.Id = await freeSql.Insert(notification).ExecuteIdentityAsync(cancellationToken);
        await auditService.WriteAsync(nameof(SystemNotification), notification.Id.ToString(), "Create", null, notification, cancellationToken);
        return ToDto(notification);
    }

    public async Task<NotificationDto> UpdateAsync(long id, UpdateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = await GetEntityAsync(id, cancellationToken);
        var before = Clone(notification);
        notification.Title = request.Title.Trim();
        notification.Content = request.Content.Trim();
        notification.Level = request.Level;
        notification.PublishAt = request.PublishAt;
        notification.ExpiresAt = request.ExpiresAt;
        notification.IsPinned = request.IsPinned;
        notification.Status = request.Status;
        notification.UpdatedAt = DateTime.UtcNow;
        await freeSql.Update<SystemNotification>().SetSource(notification).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(SystemNotification), notification.Id.ToString(), "Update", before, notification, cancellationToken);
        return ToDto(notification);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var notification = await GetEntityAsync(id, cancellationToken);
        notification.DeletedAt = DateTime.UtcNow;
        await freeSql.Update<SystemNotification>().SetSource(notification).ExecuteAffrowsAsync(cancellationToken);
        await auditService.WriteAsync(nameof(SystemNotification), notification.Id.ToString(), "Delete", notification, null, cancellationToken);
    }

    private async Task<SystemNotification> GetEntityAsync(long id, CancellationToken cancellationToken)
    {
        return await freeSql.Select<SystemNotification>().Where(x => x.Id == id && x.DeletedAt == null).FirstAsync(cancellationToken)
            ?? throw new BusinessException("NOT_FOUND", "Notification not found.");
    }

    private static NotificationDto ToDto(SystemNotification notification)
    {
        return new NotificationDto(notification.Id, notification.Title, notification.Content, notification.Level, notification.PublishAt, notification.ExpiresAt, notification.IsPinned, notification.Status, notification.CreatedAt);
    }

    private static SystemNotification Clone(SystemNotification value)
    {
        return new SystemNotification
        {
            Id = value.Id,
            Title = value.Title,
            Content = value.Content,
            Level = value.Level,
            PublishAt = value.PublishAt,
            ExpiresAt = value.ExpiresAt,
            IsPinned = value.IsPinned,
            Status = value.Status,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
            DeletedAt = value.DeletedAt
        };
    }
}
