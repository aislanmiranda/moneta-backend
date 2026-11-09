using Moneta.Domain.Entities;
using Moneta.Domain.Exceptions;

namespace Moneta.Application.TimeControl;

public class TimeControlPartyService(ITimeControlPartyRepository repository) : ITimeControlPartyService
{
    public async Task<List<TimeControlPartyDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default)
    {
        var parties = await repository.GetAllAsync(moduleId, ct);
        var usage = await repository.GetUsageCountsAsync(moduleId, ct);
        return parties
            .OrderBy(p => p.Name)
            .Select(p => ToDto(p, usage.GetValueOrDefault(p.Id)))
            .ToList();
    }

    public async Task<TimeControlPartyDto> CreateAsync(CreateTimeControlPartyDto input, CancellationToken ct = default)
    {
        var name = await ValidateNameAsync(input.ModuleId, input.Name, null, ct);

        var party = new TimeControlParty
        {
            Id = Guid.NewGuid(),
            ModuleId = input.ModuleId,
            Name = name,
            Relationship = input.Relationship,
        };

        await repository.AddAsync(party, ct);
        return ToDto(party, 0);
    }

    public async Task<TimeControlPartyDto?> UpdateAsync(Guid id, UpdateTimeControlPartyDto input, CancellationToken ct = default)
    {
        var party = await repository.GetByIdAsync(id, ct);
        if (party is null) return null;

        party.Name = await ValidateNameAsync(party.ModuleId, input.Name, party.Id, ct);
        party.Relationship = input.Relationship;

        await repository.UpdateAsync(party, ct);
        return ToDto(party, await repository.CountUsageAsync(party.Id, ct));
    }

    /// <summary>Blocked while in use — the FK is Restrict, and silently cascading would delete
    /// the user's records along with it.</summary>
    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var party = await repository.GetByIdAsync(id, ct);
        if (party is null) return false;

        var usage = await repository.CountUsageAsync(id, ct);
        if (usage > 0)
            throw new DomainValidationException(
                $"Não é possível excluir \"{party.Name}\": está em uso em {usage} item(ns).");

        await repository.RemoveAsync(party, ct);
        return true;
    }

    private async Task<string> ValidateNameAsync(Guid moduleId, string? raw, Guid? exceptId, CancellationToken ct)
    {
        var name = raw?.Trim() ?? "";
        if (name.Length is 0 or > 100)
            throw new DomainValidationException("Informe o nome (até 100 caracteres).");

        if (await repository.NameExistsAsync(moduleId, name, exceptId, ct))
            throw new DomainValidationException($"Já existe pessoa com o nome \"{name}\".");

        return name;
    }

    private static TimeControlPartyDto ToDto(TimeControlParty p, int usageCount) => new(p.Id, p.ModuleId, p.Name, p.Relationship, usageCount);
}
