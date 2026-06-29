using SunAdmin.Domain.Enums;

namespace SunAdmin.Contracts.Notifications;

public sealed record NotificationQuery(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    RecordStatus? Status = null);

public sealed record NotificationDto(
    long Id,
    string Title,
    string Content,
    NotificationLevel Level,
    DateTime? PublishAt,
    DateTime? ExpiresAt,
    bool IsPinned,
    RecordStatus Status,
    DateTime CreatedAt);

public sealed record CreateNotificationRequest(
    string Title,
    string Content,
    NotificationLevel Level,
    DateTime? PublishAt,
    DateTime? ExpiresAt,
    bool IsPinned);

public sealed record UpdateNotificationRequest(
    string Title,
    string Content,
    NotificationLevel Level,
    DateTime? PublishAt,
    DateTime? ExpiresAt,
    bool IsPinned,
    RecordStatus Status);
