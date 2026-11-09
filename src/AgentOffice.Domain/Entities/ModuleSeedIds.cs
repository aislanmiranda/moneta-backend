namespace AgentOffice.Domain.Entities;

/// <summary>Fixed ids for the two Modules that existed as hardcoded domains before the
/// generalization (Financeiro/Vida Saudável) — referenced both by the migration that backfills
/// every pre-existing row's ModuleId. New accounts get their own modules with fresh ids.</summary>
public static class ModuleSeedIds
{
    public static readonly Guid Financeiro = Guid.Parse("81ec9b19-28d9-4de1-97cb-c3411c3bf128");
    public static readonly Guid VidaSaudavel = Guid.Parse("57b10b70-2226-4e8c-8af5-efb8ce2573ad");
}
