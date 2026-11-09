using Moneta.Domain.Entities;
using Moneta.Domain.Exceptions;

namespace Moneta.Application.DateControl;

public class DateControlSourceService(
    IDateControlSourceRepository repository,
    IDateControlCategoryRepository categoryRepository
) : IDateControlSourceService
{
    public async Task<List<DateControlSourceDto>> GetAllAsync(Guid moduleId, CancellationToken ct = default) =>
        (await repository.GetAllAsync(moduleId, ct)).Select(ToDto).ToList();

    public async Task<DateControlSourceDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var source = await repository.GetByIdAsync(id, ct);
        return source is null ? null : ToDto(source);
    }

    public async Task<DateControlSourceDto> CreateAsync(CreateDateControlSourceDto input, CancellationToken ct = default)
    {
        var category = await GetCategoryAsync(input.ModuleId, input.DateControlCategoryId, ct);

        var source = new DateControlSource
        {
            Id = Guid.NewGuid(),
            ModuleId = input.ModuleId,
            Description = input.Description,
            DateControlCategoryId = category.Id,
            DateControlCategory = category,
        };

        await repository.AddAsync(source, ct);
        return ToDto(source);
    }

    public async Task<DateControlSourceDto?> UpdateAsync(
        Guid id, UpdateDateControlSourceDto input, CancellationToken ct = default)
    {
        var source = await repository.GetByIdAsync(id, ct);
        if (source is null) return null;

        var category = await GetCategoryAsync(source.ModuleId, input.DateControlCategoryId, ct);

        source.Description = input.Description;
        source.DateControlCategoryId = category.Id;
        source.DateControlCategory = category;

        await repository.UpdateAsync(source, ct);
        return ToDto(source);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken ct = default)
    {
        var source = await repository.GetByIdAsync(id, ct);
        if (source is null) return false;

        await repository.RemoveAsync(source, ct);
        return true;
    }

    /// <summary>The category must belong to the same module (the query filter already limits it
    /// to the current user's modules).</summary>
    private async Task<DateControlCategory> GetCategoryAsync(Guid moduleId, Guid categoryId, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, ct);
        if (category is null || category.ModuleId != moduleId)
            throw new NotFoundException("Categoria não encontrada.");
        return category;
    }

    private static DateControlSourceDto ToDto(DateControlSource a) => new(
        a.Id,
        a.ModuleId,
        a.Description,
        a.DateControlCategoryId,
        a.DateControlCategory.Name,
        a.DateControlCategory.Icon,
        a.Entries.Count
    );
}
