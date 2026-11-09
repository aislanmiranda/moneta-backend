namespace AgentOffice.Application.Auth;

public record RegisterDto(string FullName, string Email, string Login, string Password);

public record LoginDto(string Login, string Password);

public record UserDto(Guid Id, string FullName, string Email, string Login);

public record AuthResultDto(string Token, DateTimeOffset ExpiresAt, UserDto User);
