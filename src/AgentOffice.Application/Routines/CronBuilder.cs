using AgentOffice.Domain.Exceptions;
using Quartz;

namespace AgentOffice.Application.Routines;

/// <summary>
/// Composes a Quartz cron expression from a day-of-week set + hour set + shared minute — the
/// "modo simples" schedule builder. One pattern per activity: all selected hours share the same
/// minute, so a single 6-field cron can always express it without ambiguity (hour/minute lists in
/// Quartz cron are independent dimensions — mixing distinct minutes per hour would silently also
/// fire at unintended hour/minute combinations, so this is enforced here, not just in the UI).
/// </summary>
public static class CronBuilder
{
    private static readonly HashSet<string> ValidDays = ["MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"];

    public static string Build(string[]? daysOfWeek, int[] hours, int minute)
    {
        if (hours.Length == 0)
            throw new DomainValidationException("Selecione pelo menos um horário.");

        if (hours.Any(h => h is < 0 or > 23))
            throw new DomainValidationException("Hora inválida — use valores entre 0 e 23.");

        if (minute is < 0 or > 59)
            throw new DomainValidationException("Minuto inválido — use valores entre 0 e 59.");

        var hoursField = string.Join(",", hours.Distinct().OrderBy(h => h));

        if (daysOfWeek is null || daysOfWeek.Length == 0)
        {
            return $"0 {minute} {hoursField} * * ?";
        }

        if (daysOfWeek.Any(d => !ValidDays.Contains(d)))
            throw new DomainValidationException("Dia da semana inválido.");

        var daysField = string.Join(",", daysOfWeek.Distinct());
        return $"0 {minute} {hoursField} ? * {daysField}";
    }

    /// <summary>Resolves a ScheduleInputDto (shared by Routine and TimeControlItem) into the
    /// (CronExpression, DaysOfWeek, Hours, Minute) tuple persisted alongside the owning row —
    /// "modo avançado" (RawCron set) is validated via Quartz itself rather than reparsed here.</summary>
    public static (string Cron, string? DaysOfWeek, string? Hours, int Minute) Resolve(ScheduleInputDto input)
    {
        if (!string.IsNullOrWhiteSpace(input.RawCron))
        {
            if (!CronExpression.TryParse(input.RawCron, out _))
                throw new DomainValidationException("Expressão cron inválida.");

            return (input.RawCron, null, null, 0);
        }

        var cron = Build(input.DaysOfWeek, input.Hours, input.Minute);
        var daysOfWeek = input.DaysOfWeek is { Length: > 0 } ? string.Join(",", input.DaysOfWeek) : null;
        var hours = string.Join(",", input.Hours.Distinct().OrderBy(h => h));
        return (cron, daysOfWeek, hours, input.Minute);
    }
}
