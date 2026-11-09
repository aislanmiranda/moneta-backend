using System.Security.Claims;
using System.Text;
using AgentOffice.Application.Auth;
using AgentOffice.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgentOffice.Api.Auth;

public class JwtOptions
{
    public const string Section = "Auth";
    public const string Issuer = "agent-office";

    /// <summary>HMAC-SHA256 signing key — at least 32 characters. Set via Auth__JwtKey.</summary>
    public string JwtKey { get; set; } = "";

    /// <summary>Long-lived on purpose: it's a personal app installed on the phone.</summary>
    public int TokenDays { get; set; } = 30;

    public SymmetricSecurityKey SigningKey() => new(Encoding.UTF8.GetBytes(JwtKey));
}

public class JwtTokenService(JwtOptions options) : ITokenService
{
    public (string Token, DateTimeOffset ExpiresAt) Issue(User user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddDays(options.TokenDays);
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = JwtOptions.Issuer,
            Audience = JwtOptions.Issuer,
            Expires = expiresAt.UtcDateTime,
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
            ]),
            SigningCredentials = new SigningCredentials(options.SigningKey(), SecurityAlgorithms.HmacSha256),
        });
        return (token, expiresAt);
    }
}
