namespace Moneta.Domain;

/// <summary>
/// The user's wall-clock timezone (default America/Sao_Paulo, override with APP_TIMEZONE). Servers
/// run in UTC, so DateTime.Now / DateTimeOffset.Now can't be used for "what day/hour is it for the
/// user" — schedules saved as "09:00" mean 09:00 here, and are converted to UTC only for storage.
/// </summary>
public static class AppTime
{
    public static TimeZoneInfo Zone { get; } = Resolve();

    public static DateTimeOffset Now => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Zone);

    public static DateOnly Today => DateOnly.FromDateTime(Now.DateTime);

    /// <summary>The UTC instant of a local wall-clock date/time in the app timezone.</summary>
    public static DateTimeOffset ToUtc(DateOnly date, TimeOnly time)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, Zone), TimeSpan.Zero);
    }

    private static TimeZoneInfo Resolve()
    {
        var id = Environment.GetEnvironmentVariable("APP_TIMEZONE");
        foreach (var candidate in new[] { id, "America/Sao_Paulo", "E. South America Standard Time" })
        {
            if (string.IsNullOrWhiteSpace(candidate)) continue;
            try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.CreateCustomTimeZone("America/Sao_Paulo", TimeSpan.FromHours(-3), "Brasília", "Brasília");
    }
}
