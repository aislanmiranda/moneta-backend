namespace Moneta.Application.Auth;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto input, CancellationToken ct = default);
    Task<AuthResultDto> LoginAsync(LoginDto input, CancellationToken ct = default);
    Task<UserDto?> GetUserAsync(Guid userId, CancellationToken ct = default);
}
