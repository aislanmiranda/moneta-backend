using Moneta.Domain.Enums;

namespace Moneta.Application.Notifications;

/// <summary>Category (the pattern-level handler type) is kept for the badge/icon, but grouping
/// notifications in the UI should use ModuleId — several modules can share a pattern (e.g. two
/// DateControl modules), so Category alone isn't a fine enough key once modules are dynamic.</summary>
public record NotificationDto(
    Guid Id,
    string Message,
    DateTimeOffset? DeliveredAt,
    DateTimeOffset? ReadAt,
    RoutineType Category,
    Guid ModuleId
);

public record BulkDeleteNotificationsDto(Guid[] Ids);
