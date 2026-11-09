using Moneta.Domain.Entities;

namespace Moneta.Application.DateControl;

public interface IDateControlCategoryRepository
{
    Task<List<DateControlCategory>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateControlCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DateControlCategory entity, CancellationToken ct = default);
    Task UpdateAsync(DateControlCategory entity, CancellationToken ct = default);
    Task RemoveAsync(DateControlCategory entity, CancellationToken ct = default);
    Task<bool> NameExistsAsync(Guid moduleId, string name, Guid? exceptId, CancellationToken ct = default);
    Task<int> CountUsageAsync(Guid id, CancellationToken ct = default);
    Task<Dictionary<Guid, int>> GetUsageCountsAsync(Guid moduleId, CancellationToken ct = default);
}
