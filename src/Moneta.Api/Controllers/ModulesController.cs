using Moneta.Application.Modules;
using Microsoft.AspNetCore.Mvc;

namespace Moneta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController(IModuleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetAll(CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<ModuleDto>> Create(CreateModuleDto request, CancellationToken ct) =>
        Ok(await service.CreateAsync(request, ct));
}
