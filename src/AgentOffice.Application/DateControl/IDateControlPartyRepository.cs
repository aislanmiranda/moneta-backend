using AgentOffice.Domain.Entities;

namespace AgentOffice.Application.DateControl;

public interface IDateControlPartyRepository
{
    Task<List<DateControlParty>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateControlParty?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DateControlParty party, CancellationToken ct = default);
    Task UpdateAsync(DateControlParty party, CancellationToken ct = default);
    Task RemoveAsync(DateControlParty party, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default);
    Task<int> CountUsageAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
