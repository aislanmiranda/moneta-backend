using Moneta.Domain.Enums;

namespace Moneta.Application.Modules;

public record ModuleDto(Guid Id, string Name, string Slug, string Icon, ModulePattern Pattern, DateTimeOffset CreatedAt);

/// <summary>No Slug here — it's derived from Name server-side (see ModuleService.CreateAsync),
/// so the "Criar módulo" form only asks for the 3 things the user actually decides.</summary>
public record CreateModuleDto(string Name, string Icon, ModulePattern Pattern);
