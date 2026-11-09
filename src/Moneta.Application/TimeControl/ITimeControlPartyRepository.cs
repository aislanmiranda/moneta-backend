using Moneta.Domain.Entities;

namespace Moneta.Application.TimeControl;

public interface ITimeControlPartyRepository
{
    Task<List<TimeControlParty>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<TimeControlParty?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TimeControlParty party, CancellationToken ct = default);
    Task UpdateAsync(TimeControlParty party, CancellationToken ct = default);
    Task RemoveAsync(TimeControlParty party, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default);
    Task<int> CountUsageAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default);
}
