using Moneta.Domain.Entities;

namespace Moneta.Application.Notifications;

public class NotificationService(INotificationRepository repository) : INotificationService
{
    public async Task<List<NotificationDto>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(ToDto).ToList();

    public async Task<NotificationDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await repository.GetByIdAsync(id, ct);
        return notification is null ? null : ToDto(notification);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await repository.GetByIdAsync(id, ct);
        if (notification is null) return null;

        notification.ReadAt ??= DateTimeOffset.UtcNow;

        await repository.UpdateAsync(notification, ct);
        return ToDto(notification);
    }

    public Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        repository.GetOwnerIdAsync(id, ct);

    public Task DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        repository.DeleteManyAsync(ids, ct);

    private static NotificationDto ToDto(Notification n) =>
        new(n.Id, n.Message, n.DeliveredAt, n.ReadAt, n.Execution.Routine.Type, n.Execution.Routine.ModuleId);
}
