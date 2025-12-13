using Microsoft.AspNetCore.Http;

public class BasicAuthMiddleware
{
    private readonly RequestDelegate _next;

    // Your API login credentials
    private const string API_USER = "API";          // ← change if needed
    private const string API_PASSWORD = "API123";   // ← change if needed

    public BasicAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Protect only API routes
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            string? authHeader = context.Request.Headers["Authorization"];

            if (authHeader == null || !authHeader.StartsWith("Basic "))
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var encoded = authHeader.Substring("Basic ".Length).Trim();
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var parts = decoded.Split(':');

            if (parts.Length != 2 ||
                parts[0] != API_USER ||
                parts[1] != API_PASSWORD)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await _next(context);
    }
}
