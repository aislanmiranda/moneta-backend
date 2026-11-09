using AgentOffice.Domain.Enums;

namespace AgentOffice.Domain.Entities;

/// <summary>Records that a one-off reminder rule (BeforeDue/OnDue — see DateControlReminderJob)
/// already fired for a given entry, so a check cadence that runs several times a day doesn't
/// re-notify about the same point-in-time rule. The AfterDueRepeat rule doesn't need dedupe logic
/// beyond its own most recent dispatch row — it compares elapsed time since then.</summary>
public class DateControlReminderDispatch
{
    public Guid Id { get; set; }

    public Guid DateControlEntryId { get; set; }
    public DateControlEntry DateControlEntry { get; set; } = null!;

    public DateControlReminderRuleKind RuleKind { get; set; }
    public DateTimeOffset FiredAt { get; set; } = DateTimeOffset.UtcNow;
}
