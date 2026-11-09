using System.Text.Json;
using Moneta.Application.Modules;
using Moneta.Domain.Entities;
using Moneta.Domain.Enums;
using Moneta.Domain.Exceptions;

namespace Moneta.Application.Routines;

public class RoutineService(
    IRoutineRepository repository,
    IModuleRepository moduleRepository
) : IRoutineService
{
    private const string TimeZone = "UTC";
    private static readonly JsonSerializerOptions PayloadJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<List<RoutineDto>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(ToDto).ToList();

    public async Task<RoutineDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var routine = await repository.GetByIdAsync(id, ct);
        return routine is null ? null : ToDto(routine);
    }

    public async Task<RoutineDto> CreateAsync(CreateRoutineDto input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new DomainValidationException("Informe o título da rotina.");

        var module = await moduleRepository.GetByIdAsync(input.ModuleId, ct)
            ?? throw new NotFoundException("Módulo não encontrado.");

        var type = ResolveType(module.Pattern);
        var resolved = CronBuilder.Resolve(input.Schedule);

        var routine = new Routine
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            ModuleId = module.Id,
            Module = module,
            Type = type,
            Payload = BuildReminderRules(type, input.ReminderRules),
            Status = RoutineStatus.Active,
            CronExpression = resolved.Cron,
            TimeZone = TimeZone,
            DaysOfWeek = resolved.DaysOfWeek,
            Hours = resolved.Hours,
            Minute = resolved.Minute,
            // Fire once right away so the effect is visible without waiting for the first cron
            // tick — ScheduleSyncJob consumes this on its next poll.
            TriggerRequestedAt = DateTimeOffset.UtcNow,
        };

        await repository.AddAsync(routine, ct);
        return ToDto(routine);
    }

    /// <summary>Which RoutineType/Job handles a Module's reminders — one per ModulePattern,
    /// shared by every Module of that pattern (see RoutineType's doc comment in Enums.cs).</summary>
    private static RoutineType ResolveType(ModulePattern pattern) => pattern switch
    {
        ModulePattern.DateControl => RoutineType.DateControlReminder,
        ModulePattern.TimeControl => RoutineType.TimeControlReminder,
        ModulePattern.DateTimeControl => RoutineType.DateTimeControlReminder,
        _ => throw new ArgumentOutOfRangeException(nameof(pattern)),
    };

    public async Task<RoutineDto?> UpdateAsync(Guid id, UpdateRoutineDto input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            throw new DomainValidationException("Informe o título da rotina.");

        var routine = await repository.GetByIdAsync(id, ct);
        if (routine is null) return null;

        var resolved = CronBuilder.Resolve(input.Schedule);

        routine.Title = input.Title.Trim();
        routine.Payload = BuildReminderRules(routine.Type, input.ReminderRules);

        if (routine.CronExpression != resolved.Cron)
        {
            routine.NextRunAt = null; // worker recomputes on its next sync
        }

        routine.CronExpression = resolved.Cron;
        routine.DaysOfWeek = resolved.DaysOfWeek;
        routine.Hours = resolved.Hours;
        routine.Minute = resolved.Minute;

        await repository.UpdateAsync(routine, ct);
        return ToDto(routine);
    }

    public async Task<RoutineDto?> UpdateStatusAsync(Guid id, RoutineStatus status, CancellationToken ct = default)
    {
        var routine = await repository.GetByIdAsync(id, ct);
        if (routine is null) return null;

        routine.Status = status;
        if (status == RoutineStatus.Paused) routine.NextRunAt = null;

        await repository.UpdateAsync(routine, ct);
        return ToDto(routine);
    }

    /// <summary>"Executar agora": only records the request — ScheduleSyncJob picks it up on its
    /// next poll (up to 30s later) and fires the routine out-of-band. API and worker never call
    /// each other directly.</summary>
    public async Task<RoutineDto?> RequestTriggerAsync(Guid id, CancellationToken ct = default)
    {
        var routine = await repository.GetByIdAsync(id, ct);
        if (routine is null) return null;

        routine.TriggerRequestedAt = DateTimeOffset.UtcNow;

        await repository.UpdateAsync(routine, ct);
        return ToDto(routine);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var routine = await repository.GetByIdAsync(id, ct);
        if (routine is null) return false;

        await repository.RemoveAsync(routine, ct);
        return true;
    }

    private static string? BuildReminderRules(RoutineType type, ReminderRulesDto? rules) => type switch
    {
        RoutineType.DateControlReminder when rules is not null => JsonSerializer.Serialize(rules, PayloadJsonOptions),
        _ => null,
    };

    public static ReminderRulesDto? ReadReminderRules(Routine routine)
    {
        if (routine.Type != RoutineType.DateControlReminder || routine.Payload is null) return null;
        return JsonSerializer.Deserialize<ReminderRulesDto>(routine.Payload, PayloadJsonOptions);
    }

    private static RoutineDto ToDto(Routine r)
    {
        var isAdvanced = r.Hours is null;

        return new RoutineDto(
            r.Id,
            r.Title,
            r.ModuleId,
            r.Module.Name,
            r.Type,
            r.DaysOfWeek?.Split(',', StringSplitOptions.RemoveEmptyEntries),
            isAdvanced
                ? []
                : r.Hours!.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray(),
            r.Minute,
            r.CronExpression,
            isAdvanced,
            r.NextRunAt,
            r.Executions.OrderByDescending(e => e.StartedAt).FirstOrDefault()?.StartedAt,
            r.TriggerRequestedAt,
            ReadReminderRules(r),
            r.Status
        );
    }
}
