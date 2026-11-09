using Moneta.Application.Notifications;
using Moneta.Domain.Entities;
using Moneta.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Moneta.Repository.Notifications;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public Task<List<Notification>> GetAllAsync(CancellationToken ct = default) =>
        db.Notifications.AsNoTracking()
            .Include(n => n.Execution).ThenInclude(e => e.Routine)
            .OrderByDescending(n => n.DeliveredAt).ToListAsync(ct);

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Notifications.Include(n => n.Execution).ThenInclude(e => e.Routine)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
    {
        db.Notifications.Update(notification);
        await db.SaveChangesAsync(ct);
    }

    public Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        db.Notifications.Where(n => n.Id == id)
            .Select(n => n.Execution.Routine.Module.UserId)
            .FirstOrDefaultAsync(ct);

    public Task DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        db.Notifications.Where(n => ids.Contains(n.Id)).ExecuteDeleteAsync(ct);
}
