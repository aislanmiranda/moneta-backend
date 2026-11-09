namespace AgentOffice.Application.Notifications;

public interface INotificationService
{
    Task<List<NotificationDto>> GetAllAsync(CancellationToken ct = default);
    Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<NotificationDto?> MarkAsReadAsync(Guid id, CancellationToken ct = default);
    /// <summary>Who the notification belongs to (its routine's module owner).</summary>
    Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default);
    Task DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
