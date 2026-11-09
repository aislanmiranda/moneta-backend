namespace AgentOffice.Domain.Auth;

/// <summary>Who the current operation runs for. The API resolves it from the JWT; background
/// processes (schedule/, the API's notification relay) run as <see cref="IsSystem"/>, which sees
/// every user's data. AppDbContext's query filters read this on every query.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsSystem { get; }
}

/// <summary>For processes that legitimately work across all users (the scheduler worker).</summary>
public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? UserId => null;
    public bool IsSystem => true;
}
