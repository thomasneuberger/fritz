namespace Fritz.Api.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private const string ApiKeyHeaderName = "X-Api-Key";

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for OpenAPI and Scalar UI in development
        if (context.Request.Path.StartsWithSegments("/openapi") || 
            context.Request.Path.StartsWithSegments("/scalar"))
        {
            await _next(context);
            return;
        }

        // Allow OPTIONS requests for CORS without API key
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        // Check for API key in header first, then in query string (for SignalR)
        string? extractedApiKey = null;
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerApiKey))
        {
            extractedApiKey = headerApiKey.ToString();
        }
        else if (context.Request.Query.TryGetValue("apiKey", out var queryApiKey))
        {
            extractedApiKey = queryApiKey.ToString();
        }

        if (string.IsNullOrEmpty(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        var apiKey = _configuration.GetValue<string>("ApiKey");

        if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
