using AgentOffice.Domain.Entities;

namespace AgentOffice.Application.Auth;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);

    /// <summary>Modules created before accounts existed (UserId null) — invisible to everyone
    /// until the first account claims them.</summary>
    Task<bool> HasUnownedModulesAsync(CancellationToken ct = default);

    /// <summary>The unowned modules (see HasUnownedModulesAsync), each flagged with whether it
    /// already has any party (centro de custo / pessoa).</summary>
    Task<List<(Module Module, bool HasParties)>> GetUnownedModulesAsync(CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);

    /// <summary>Creates the account plus its starter data in one transaction: either claims every
    /// module that predates accounts (first account only) or gets <paramref name="starterModules"/>
    /// with their <paramref name="starterRecords"/> (parties, categories, types).</summary>
    Task AddWithModulesAsync(User user, bool claimUnownedModules, IReadOnlyList<Module> starterModules,
        IReadOnlyList<object> starterRecords, CancellationToken ct = default);
}
