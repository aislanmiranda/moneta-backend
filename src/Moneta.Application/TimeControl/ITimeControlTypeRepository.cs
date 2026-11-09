using Moneta.Domain.Entities;

namespace Moneta.Application.TimeControl;

public interface ITimeControlTypeRepository
{
    Task<List<TimeControlType>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<TimeControlType?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TimeControlType entity, CancellationToken ct = default);
    Task UpdateAsync(TimeControlType entity, CancellationToken ct = default);
    Task RemoveAsync(TimeControlType entity, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default);
    Task<int> CountUsageAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default);
}
