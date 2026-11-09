using AgentOffice.Infra.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AgentOffice.Api.Auth;

/// <summary>
/// Every route under /api/modules/{moduleId}/... must target one of the caller's modules. The
/// query filters already hide other users' rows on reads; this also covers writes, where a
/// service would otherwise insert a row into whatever moduleId the URL carries.
/// </summary>
public class ModuleOwnershipFilter(AppDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values.TryGetValue("moduleId", out var raw) &&
            Guid.TryParse(raw?.ToString(), out var moduleId) &&
            !await db.Modules.AnyAsync(m => m.Id == moduleId, context.HttpContext.RequestAborted))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
