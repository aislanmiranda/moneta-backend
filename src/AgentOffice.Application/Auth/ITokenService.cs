using AgentOffice.Domain.Entities;

namespace AgentOffice.Application.Auth;

/// <summary>Issues the bearer token for a signed-in user — implemented in the API, which owns the
/// signing key and JWT settings.</summary>
public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) Issue(User user);
}
