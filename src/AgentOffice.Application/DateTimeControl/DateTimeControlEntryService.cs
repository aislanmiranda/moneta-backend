using AgentOffice.Domain.Entities;
using AgentOffice.Domain.Enums;

namespace AgentOffice.Application.DateTimeControl;

public class DateTimeControlEntryService(IDateTimeControlEntryRepository repository) : IDateTimeControlEntryService
{
    public async Task<List<DateTimeControlEntryDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        (await repository.GetAllAsync(moduleId, ct)).Select(ToDto).ToList();

    public async Task<DateTimeControlEntryDto> CreateAsync(
        CreateDateTimeControlEntryDto input, CancellationToken ct = default)
    {
        var entry = new DateTimeControlEntry
        {
            Id = Guid.NewGuid(),
            ModuleId = input.ModuleId,
            Title = input.Title,
            // Npgsql only writes timestamptz with offset 0 — callers may send any offset (e.g. -03:00).
            ScheduledAt = input.ScheduledAt.ToUniversalTime(),
            DurationMinutes = input.DurationMinutes,
            Notes = input.Notes,
            Status = DateTimeControlEntryStatus.Scheduled,
        };

        await repository.AddAsync(entry, ct);
        return ToDto(entry);
    }

    public async Task<DateTimeControlEntryDto?> UpdateAsync(
        Guid id, UpdateDateTimeControlEntryDto input, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        if (entry is null) return null;

        entry.Title = input.Title;
        entry.ScheduledAt = input.ScheduledAt.ToUniversalTime();
        entry.DurationMinutes = input.DurationMinutes;
        entry.Notes = input.Notes;

        await repository.UpdateAsync(entry, ct);
        return ToDto(entry);
    }

    public async Task<DateTimeControlEntryDto?> UpdateStatusAsync(
        Guid id, DateTimeControlEntryStatus status, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        if (entry is null) return null;

        entry.Status = status;

        await repository.UpdateAsync(entry, ct);
        return ToDto(entry);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var entry = await repository.GetByIdAsync(id, ct);
        if (entry is null) return false;

        await repository.RemoveAsync(entry, ct);
        return true;
    }

    private static DateTimeControlEntryDto ToDto(DateTimeControlEntry e) => new(
        e.Id, e.ModuleId, e.Title, e.ScheduledAt, e.DurationMinutes, e.Notes, e.Status
    );
}
