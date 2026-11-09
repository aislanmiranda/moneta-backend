using Moneta.Domain.Entities;

namespace Moneta.Application.Routines;

public interface IRoutineRepository
{
    Task<List<Routine>> GetAllAsync(CancellationToken ct = default);
    Task<Routine?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Routine routine, CancellationToken ct = default);
    Task UpdateAsync(Routine routine, CancellationToken ct = default);
    Task RemoveAsync(Routine routine, CancellationToken ct = default);
}
