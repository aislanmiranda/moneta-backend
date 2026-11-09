using AgentOffice.Domain.Entities;

namespace AgentOffice.Application.Notifications;

public interface INotificationRepository
{
    Task<List<Notification>> GetAllAsync(CancellationToken ct = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Notification notification, CancellationToken ct = default);
    Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default);
    Task DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
