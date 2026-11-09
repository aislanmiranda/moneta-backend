using AgentOffice.Application.DateControl;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Controllers;

[ApiController]
[Route("api/modules/{moduleId:guid}/categories")]
public class DateControlCategoriesController(IDateControlCategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DateControlCategoryDto>>> GetAll(Guid moduleId, CancellationToken ct) =>
        Ok(await service.GetAllAsync(moduleId, ct));

    [HttpPost]
    public async Task<ActionResult<DateControlCategoryDto>> Create(Guid moduleId, SaveDateControlCategoryDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(moduleId, request, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DateControlCategoryDto>> Update(Guid moduleId, Guid id, SaveDateControlCategoryDto request, CancellationToken ct)
    {
        var entity = await service.UpdateAsync(id, request, ct);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid moduleId, Guid id, CancellationToken ct) =>
        await service.RemoveAsync(id, ct) ? NoContent() : NotFound();
}
