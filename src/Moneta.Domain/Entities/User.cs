namespace Moneta.Domain.Entities;

/// <summary>An account. Owns its Modules — and through them every record in the app (entries,
/// parties, items, routines, notifications); see AppDbContext's query filters.</summary>
public class User
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }

    /// <summary>Not used to sign in — kept for future notifications/recovery e-mails.</summary>
    public required string Email { get; set; }

    /// <summary>Sign-in name, stored lowercase (unique, case-insensitive).</summary>
    public required string Login { get; set; }

    /// <summary>PBKDF2 hash of the 6-digit numeric password (ASP.NET Identity PasswordHasher).</summary>
    public required string PasswordHash { get; set; }

    /// <summary>A 6-digit PIN only has 10^6 combinations — consecutive failures lock the account
    /// for a while (see AuthService) so it can't be brute-forced online.</summary>
    public int FailedLoginCount { get; set; }
    public DateTimeOffset? LockoutUntil { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
