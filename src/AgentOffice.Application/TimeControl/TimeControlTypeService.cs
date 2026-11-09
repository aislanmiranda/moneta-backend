using AgentOffice.Domain.Entities;
using AgentOffice.Domain.Exceptions;

namespace AgentOffice.Application.TimeControl;

public class TimeControlTypeService(ITimeControlTypeRepository repository) : ITimeControlTypeService
{
    public async Task<List<TimeControlTypeDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default)
    {
        var all = await repository.GetAllAsync(moduleId, ct);
        var usage = await repository.GetUsageCountsAsync(moduleId, ct);
        return all.Select(x => ToDto(x, usage.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<TimeControlTypeDto> CreateAsync(Guid moduleId, SaveTimeControlTypeDto input, CancellationToken ct = default)
    {
        var entity = new TimeControlType
        {
            Id = Guid.NewGuid(),
            ModuleId = moduleId,
            Name = await ValidateNameAsync(moduleId, input.Name, null, ct),
            Icon = ValidateIcon(input.Icon),
        };

        await repository.AddAsync(entity, ct);
        return ToDto(entity, 0);
    }

    public async Task<TimeControlTypeDto?> UpdateAsync(Guid id, SaveTimeControlTypeDto input, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null) return null;

        entity.Name = await ValidateNameAsync(entity.ModuleId, input.Name, entity.Id, ct);
        entity.Icon = ValidateIcon(input.Icon);

        await repository.UpdateAsync(entity, ct);
        return ToDto(entity, await repository.CountUsageAsync(entity.Id, ct));
    }

    /// <summary>Blocked while in use — the FK is Restrict, and cascading would delete the
    /// user's records along with it.</summary>
    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await repository.GetByIdAsync(id, ct);
        if (entity is null) return false;

        var usage = await repository.CountUsageAsync(id, ct);
        if (usage > 0)
            throw new DomainValidationException(
                $"Não é possível excluir \"{entity.Name}\": está em uso em {usage} item(ns).");

        await repository.RemoveAsync(entity, ct);
        return true;
    }

    private async Task<string> ValidateNameAsync(Guid moduleId, string? raw, Guid? exceptId, CancellationToken ct)
    {
        var name = raw?.Trim() ?? "";
        if (name.Length is 0 or > 100)
            throw new DomainValidationException("Informe o nome (até 100 caracteres).");

        if (await repository.NameExistsAsync(moduleId, name, exceptId, ct))
            throw new DomainValidationException($"Já existe tipo com o nome \"{name}\".");

        return name;
    }

    // Any Lucide icon name is accepted; the frontend falls back to a generic icon for unknown ones.
    private static string ValidateIcon(string? raw)
    {
        var icon = raw?.Trim() ?? "";
        if (icon.Length is 0 or > 50)
            throw new DomainValidationException("Escolha um ícone.");
        return icon;
    }

    private static TimeControlTypeDto ToDto(TimeControlType x, int usageCount) => new(x.Id, x.ModuleId, x.Name, x.Icon, usageCount);
}
