namespace ClaimsModule.API.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
            context.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-ID"] = context.Request.Headers["X-Correlation-ID"];
        await next(context);
    }
}
