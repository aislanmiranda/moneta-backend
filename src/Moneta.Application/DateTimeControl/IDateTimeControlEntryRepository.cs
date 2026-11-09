using Moneta.Domain.Entities;

namespace Moneta.Application.DateTimeControl;

public interface IDateTimeControlEntryRepository
{
    Task<List<DateTimeControlEntry>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateTimeControlEntry?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DateTimeControlEntry entry, CancellationToken ct = default);
    Task UpdateAsync(DateTimeControlEntry entry, CancellationToken ct = default);
    Task RemoveAsync(DateTimeControlEntry entry, CancellationToken ct = default);
}
