using System.Net.Mail;
using System.Text.RegularExpressions;
using AgentOffice.Domain.Entities;
using AgentOffice.Domain.Enums;
using AgentOffice.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace AgentOffice.Application.Auth;

public partial class AuthService(
    IUserRepository repository,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher
) : IAuthService
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    // Same message for unknown login and wrong password — doesn't reveal which logins exist.
    private const string InvalidCredentials = "Login ou senha inválidos.";

    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex SixDigits();

    [GeneratedRegex(@"^[a-z0-9._-]{3,50}$")]
    private static partial Regex ValidLogin();

    public async Task<AuthResultDto> RegisterAsync(RegisterDto input, CancellationToken ct = default)
    {
        var fullName = input.FullName?.Trim() ?? "";
        var email = input.Email?.Trim() ?? "";
        var login = NormalizeLogin(input.Login);

        if (fullName.Length is < 3 or > 150)
            throw new DomainValidationException("Informe o nome completo.");

        if (email.Length > 200 || !MailAddress.TryCreate(email, out _))
            throw new DomainValidationException("Informe um e-mail válido.");

        if (!ValidLogin().IsMatch(login))
            throw new DomainValidationException(
                "O login deve ter de 3 a 50 caracteres: letras, números, ponto, hífen ou sublinhado.");

        if (!SixDigits().IsMatch(input.Password ?? ""))
            throw new DomainValidationException("A senha deve ter exatamente 6 números.");

        if (await repository.GetByLoginAsync(login, ct) is not null)
            throw new DomainValidationException("Esse login já está em uso.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            Login = login,
            PasswordHash = "",
        };
        user.PasswordHash = passwordHasher.HashPassword(user, input.Password!);

        // The first account inherits every module created before accounts existed (the user's
        // data, or the two modules an early migration seeded on a fresh database). Every account
        // — the first one included — then gets whichever standard modules it still lacks, and a
        // "self" party in each date/time module that has none, so it's usable right away.
        var claimed = !await repository.AnyAsync(ct) ? await repository.GetUnownedModulesAsync(ct) : [];
        var isFirstAccount = claimed.Count > 0;
        var firstName = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

        var claimedPatterns = claimed.Select(c => c.Module.Pattern).ToHashSet();
        List<Module> modules = StarterModules(user.Id).Where(m => !claimedPatterns.Contains(m.Pattern)).ToList();

        var records = new List<object>();
        foreach (var module in modules)
        {
            records.AddRange(ModuleDefaults.StarterRecordsFor(module));
            records.AddRange(SelfPartyFor(module, firstName));
        }
        foreach (var (module, hasParties) in claimed)
        {
            if (!hasParties) records.AddRange(SelfPartyFor(module, firstName));
        }

        await repository.AddWithModulesAsync(user, isFirstAccount, modules, records, ct);
        return Issue(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginDto input, CancellationToken ct = default)
    {
        var user = await repository.GetByLoginAsync(NormalizeLogin(input.Login), ct)
            ?? throw new DomainValidationException(InvalidCredentials);

        var now = DateTimeOffset.UtcNow;
        if (user.LockoutUntil > now)
        {
            var minutes = (int)Math.Ceiling((user.LockoutUntil.Value - now).TotalMinutes);
            throw new DomainValidationException(
                $"Muitas tentativas erradas. Tente de novo em {minutes} minuto{(minutes > 1 ? "s" : "")}.");
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Password ?? "");
        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.FailedLoginCount = 0;
                user.LockoutUntil = now + LockoutDuration;
            }
            await repository.UpdateAsync(user, ct);
            throw new DomainValidationException(InvalidCredentials);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = passwordHasher.HashPassword(user, input.Password!);

        user.FailedLoginCount = 0;
        user.LockoutUntil = null;
        await repository.UpdateAsync(user, ct);

        return Issue(user);
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await repository.GetByIdAsync(userId, ct);
        return user is null ? null : ToDto(user);
    }

    private AuthResultDto Issue(User user)
    {
        var (token, expiresAt) = tokenService.Issue(user);
        return new AuthResultDto(token, expiresAt, ToDto(user));
    }

    private static string NormalizeLogin(string? login) => (login ?? "").Trim().ToLowerInvariant();

    private static List<Module> StarterModules(Guid userId) =>
    [
        new() { Id = Guid.NewGuid(), UserId = userId, Name = "Financeiro", Slug = "financeiro", Icon = "Wallet", Pattern = ModulePattern.DateControl },
        new() { Id = Guid.NewGuid(), UserId = userId, Name = "Vida Saudável", Slug = "vida-saudavel", Icon = "HeartPulse", Pattern = ModulePattern.TimeControl },
        new() { Id = Guid.NewGuid(), UserId = userId, Name = "Agenda", Slug = "agenda", Icon = "Calendar", Pattern = ModulePattern.DateTimeControl },
    ];

    private static IEnumerable<object> SelfPartyFor(Module module, string name) => module.Pattern switch
    {
        ModulePattern.DateControl => [new DateControlParty { Id = Guid.NewGuid(), ModuleId = module.Id, Name = name, Relationship = PartyRelationship.Self }],
        ModulePattern.TimeControl => [new TimeControlParty { Id = Guid.NewGuid(), ModuleId = module.Id, Name = name, Relationship = PartyRelationship.Self }],
        _ => [],
    };

    private static UserDto ToDto(User u) => new(u.Id, u.FullName, u.Email, u.Login);
}
