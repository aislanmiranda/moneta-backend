using AgentOffice.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AgentOffice.Api.Middleware;

/// <summary>Maps domain exceptions to HTTP responses so controllers stay free of try/catch noise.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainValidationException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Erro interno inesperado.");
        }
    }

    private static Task WriteProblem(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = message,
        });
    }
}
