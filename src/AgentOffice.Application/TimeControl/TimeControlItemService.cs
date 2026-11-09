using AgentOffice.Application.Routines;
using AgentOffice.Domain.Entities;
using AgentOffice.Domain.Enums;
using AgentOffice.Domain.Exceptions;

namespace AgentOffice.Application.TimeControl;

public class TimeControlItemService(
    ITimeControlItemRepository repository,
    ITimeControlPartyRepository partyRepository,
    ITimeControlTypeRepository typeRepository
) : ITimeControlItemService
{
    public async Task<List<TimeControlItemDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        (await repository.GetAllAsync(moduleId, ct)).Select(ToDto).ToList();

    public async Task<TimeControlItemDto> CreateAsync(CreateTimeControlItemDto input, CancellationToken ct = default)
    {
        var party = await partyRepository.GetByIdAsync(input.TimeControlPartyId, ct)
            ?? throw new NotFoundException("Pessoa não encontrada.");

        var type = await GetTypeAsync(input.ModuleId, input.TimeControlTypeId, ct);
        var resolved = CronBuilder.Resolve(input.Schedule);

        var item = new TimeControlItem
        {
            Id = Guid.NewGuid(),
            ModuleId = input.ModuleId,
            Title = input.Title,
            TimeControlTypeId = type.Id,
            TimeControlType = type,
            TimeControlPartyId = input.TimeControlPartyId,
            TimeControlParty = party,
            Dosage = NormalizeDosage(input.Dosage),
            Status = TimeControlItemStatus.Active,
            CronExpression = resolved.Cron,
            DaysOfWeek = resolved.DaysOfWeek,
            Hours = resolved.Hours,
            Minute = resolved.Minute,
        };

        await repository.AddAsync(item, ct);
        return ToDto(item);
    }

    public async Task<TimeControlItemDto?> UpdateAsync(Guid id, UpdateTimeControlItemDto input, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item is null) return null;

        var party = await partyRepository.GetByIdAsync(input.TimeControlPartyId, ct)
            ?? throw new NotFoundException("Pessoa não encontrada.");

        var type = await GetTypeAsync(item.ModuleId, input.TimeControlTypeId, ct);
        var resolved = CronBuilder.Resolve(input.Schedule);

        item.Title = input.Title;
        item.TimeControlTypeId = type.Id;
        item.TimeControlType = type;
        item.TimeControlPartyId = input.TimeControlPartyId;
        item.TimeControlParty = party;
        item.Dosage = NormalizeDosage(input.Dosage);
        item.CronExpression = resolved.Cron;
        item.DaysOfWeek = resolved.DaysOfWeek;
        item.Hours = resolved.Hours;
        item.Minute = resolved.Minute;

        await repository.UpdateAsync(item, ct);
        return ToDto(item);
    }

    public async Task<TimeControlItemDto?> UpdateStatusAsync(
        Guid id, TimeControlItemStatus status, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item is null) return null;

        item.Status = status;

        await repository.UpdateAsync(item, ct);
        return ToDto(item);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item is null) return false;

        await repository.RemoveAsync(item, ct);
        return true;
    }

    private async Task<TimeControlType> GetTypeAsync(Guid moduleId, Guid typeId, CancellationToken ct)
    {
        var type = await typeRepository.GetByIdAsync(typeId, ct);
        if (type is null || type.ModuleId != moduleId)
            throw new NotFoundException("Tipo não encontrado.");
        return type;
    }

    private static string? NormalizeDosage(string? dosage) =>
        string.IsNullOrWhiteSpace(dosage) ? null : dosage.Trim();

    private static TimeControlItemDto ToDto(TimeControlItem i)
    {
        var isAdvanced = i.Hours is null;

        return new TimeControlItemDto(
            i.Id,
            i.ModuleId,
            i.Title,
            i.TimeControlTypeId,
            i.TimeControlType.Name,
            i.TimeControlType.Icon,
            i.TimeControlPartyId,
            i.TimeControlParty.Name,
            i.Dosage,
            i.Status,
            i.DaysOfWeek?.Split(',', StringSplitOptions.RemoveEmptyEntries),
            isAdvanced
                ? []
                : i.Hours!.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray(),
            i.Minute,
            i.CronExpression,
            isAdvanced
        );
    }
}
