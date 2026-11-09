using Moneta.Domain.Entities;

namespace Moneta.Application.DateControl;

public interface IDateControlSourceRepository
{
    Task<List<DateControlSource>> GetAllAsync(Guid moduleId, CancellationToken ct = default);
    Task<DateControlSource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DateControlSource source, CancellationToken ct = default);
    Task UpdateAsync(DateControlSource source, CancellationToken ct = default);
    Task RemoveAsync(DateControlSource source, CancellationToken ct = default);
}
