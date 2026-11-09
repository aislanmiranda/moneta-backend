using System.Security.Claims;
using Moneta.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterDto request, CancellationToken ct) =>
        Ok(await service.RegisterAsync(request, ct));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginDto request, CancellationToken ct) =>
        Ok(await service.LoginAsync(request, ct));

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await service.GetUserAsync(id, ct);
        return user is null ? Unauthorized() : Ok(user);
    }
}
