namespace AgentOffice.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid ExecutionId { get; set; }
    public RoutineExecution Execution { get; set; } = null!;

    public required string Message { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }
}
