using Moneta.Domain.Entities;

namespace Moneta.Application.TimeControl;

public interface ITimeControlItemRepository
{
    Task<List<TimeControlItem>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<TimeControlItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TimeControlItem item, CancellationToken ct = default);
    Task UpdateAsync(TimeControlItem item, CancellationToken ct = default);
    Task RemoveAsync(TimeControlItem item, CancellationToken ct = default);
}
