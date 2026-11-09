using System.Globalization;
using System.Text;
using AgentOffice.Domain.Auth;
using AgentOffice.Domain.Entities;

namespace AgentOffice.Application.Modules;

public class ModuleService(IModuleRepository repository, ICurrentUser currentUser) : IModuleService
{
    public async Task<List<ModuleDto>> GetAllAsync(CancellationToken ct = default) =>
        (await repository.GetAllAsync(ct)).Select(ToDto).ToList();

    public async Task<ModuleDto> CreateAsync(CreateModuleDto input, CancellationToken ct = default)
    {
        var slug = await GenerateUniqueSlugAsync(input.Name, ct);

        var module = new Module
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId ?? throw new InvalidOperationException("Criar módulo exige usuário autenticado."),
            Name = input.Name,
            Slug = slug,
            Icon = input.Icon,
            Pattern = input.Pattern,
        };

        await repository.AddAsync(module, ModuleDefaults.StarterRecordsFor(module), ct);
        return ToDto(module);
    }

    /// <summary>Appends -2, -3, ... on collision rather than failing — module names aren't
    /// required to be unique, only the routable slug derived from them (per user: the query
    /// filter scopes SlugExistsAsync to the current user's modules).</summary>
    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await repository.SlugExistsAsync(slug, ct))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string Slugify(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(c)) builder.Append(char.ToLowerInvariant(c));
            else if (builder.Length > 0 && builder[^1] != '-') builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }

    private static ModuleDto ToDto(Module m) => new(m.Id, m.Name, m.Slug, m.Icon, m.Pattern, m.CreatedAt);
}
