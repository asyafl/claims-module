using ClaimsModule.Domain.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace ClaimsModule.API.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation error: {Errors}", ex.Message);
            context.Response.StatusCode = 422;
            context.Response.ContentType = "application/json";
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "ValidationError",
                title = "One or more validation errors occurred.",
                status = 422,
                errors
            }));
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Domain error: {Message}", ex.Message);
            context.Response.StatusCode = 422;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "DomainError",
                title = ex.Message,
                status = 422
            }));
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "NotFound",
                title = ex.Message,
                status = 404
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "InternalServerError",
                title = "An unexpected error occurred.",
                status = 500
            }));
        }
    }
}
