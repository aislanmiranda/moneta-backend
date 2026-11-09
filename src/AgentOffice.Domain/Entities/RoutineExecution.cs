using AgentOffice.Domain.Enums;

namespace AgentOffice.Domain.Entities;

public class RoutineExecution
{
    public Guid Id { get; set; }

    public Guid RoutineId { get; set; }
    public Routine Routine { get; set; } = null!;

    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAt { get; set; }
    public RoutineExecutionStatus Status { get; set; } = RoutineExecutionStatus.Running;
    public string? Result { get; set; }

    public ICollection<Notification> Notifications { get; set; } = [];
}
