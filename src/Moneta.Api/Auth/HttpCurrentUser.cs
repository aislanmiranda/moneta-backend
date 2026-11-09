using System.Security.Claims;
using Moneta.Domain.Auth;

namespace Moneta.Api.Auth;

/// <summary>The signed-in user from the request's JWT. Outside a request (background services)
/// there's no user, so nothing is visible unless the service opts into <see cref="RunAsSystem"/>.</summary>
public class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private bool _system;

    public Guid? UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public bool IsSystem => _system;

    /// <summary>For API-hosted background work that must see every user's data (the Postgres →
    /// SignalR notification relay). Scoped: affects only the current DI scope.</summary>
    public void RunAsSystem() => _system = true;
}
