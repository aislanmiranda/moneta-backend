using Moneta.Domain.Enums;

namespace Moneta.Domain.Entities;

/// <summary>Starter categories/types every new module gets, so it's usable right away — the user
/// edits them afterwards in the module's settings. Same names/icons the fixed lists used to have
/// (the migration that made them editable seeds existing modules with these too).</summary>
public static class ModuleDefaults
{
    public static readonly IReadOnlyList<(string Name, string Icon)> DateControlCategories =
    [
        ("Internet", "Wifi"),
        ("Energia", "Zap"),
        ("Água", "Droplet"),
        ("Cartão", "CreditCard"),
        ("Assinatura", "Tv"),
        ("Aluguel", "Home"),
        ("Outro", "MoreHorizontal"),
        ("Saúde", "Stethoscope"),
        ("Lazer", "Palmtree"),
    ];

    public static readonly IReadOnlyList<(string Name, string Icon)> TimeControlTypes =
    [
        ("Remédio", "Pill"),
        ("Água", "Droplet"),
        ("Academia", "Dumbbell"),
    ];

    /// <summary>Category/type rows to create alongside a new module (empty for patterns without any).</summary>
    public static IEnumerable<object> StarterRecordsFor(Module module) => module.Pattern switch
    {
        ModulePattern.DateControl => DateControlCategories.Select(c => (object)new DateControlCategory
        {
            Id = Guid.NewGuid(), ModuleId = module.Id, Name = c.Name, Icon = c.Icon,
        }),
        ModulePattern.TimeControl => TimeControlTypes.Select(t => (object)new TimeControlType
        {
            Id = Guid.NewGuid(), ModuleId = module.Id, Name = t.Name, Icon = t.Icon,
        }),
        _ => [],
    };
}
