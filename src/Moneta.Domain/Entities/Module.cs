using Moneta.Domain.Enums;

namespace Moneta.Domain.Entities;

/// <summary>
/// A user-created domain instance (e.g. "Financeiro", "Vida Saudável", "Agenda") following one of
/// the fixed ModulePatterns. The identity that used to be implicit/hardcoded per domain (which
/// table, which controller, which menu entry) now lives as a row here — adding a new module of an
/// existing pattern requires no new code, just a new Module row.
/// </summary>
public class Module
{
    public Guid Id { get; set; }

    /// <summary>Owner. Nullable only for rows created before accounts existed — the first account
    /// registered claims them (AuthService.RegisterAsync); every module created since has one.</summary>
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public required string Name { get; set; }

    /// <summary>URL-safe identifier used for the sidebar route (e.g. "financeiro"), unique per user.</summary>
    public required string Slug { get; set; }

    /// <summary>Name of a known Lucide icon, resolved client-side (e.g. "Wallet").</summary>
    public required string Icon { get; set; }

    public ModulePattern Pattern { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
