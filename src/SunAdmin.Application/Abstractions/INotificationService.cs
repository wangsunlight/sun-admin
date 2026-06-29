using SunAdmin.Contracts.Common;
using SunAdmin.Contracts.Notifications;

namespace SunAdmin.Application.Abstractions;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetPageAsync(NotificationQuery query, CancellationToken cancellationToken = default);
    Task<NotificationDto> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<NotificationDto> UpdateAsync(long id, UpdateNotificationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
}
