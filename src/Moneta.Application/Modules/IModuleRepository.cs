using Moneta.Domain.Entities;

namespace Moneta.Application.Modules;

public interface IModuleRepository
{
    Task<List<Module>> GetAllAsync(CancellationToken ct = default);
    Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Adds the module together with its starter records (categories/types) in one save.</summary>
    Task AddAsync(Module module, IEnumerable<object> starterRecords, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}
