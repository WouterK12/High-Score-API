namespace HighScoreAPI.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;

    private const string ContentTypePlainText = "text/plain";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderNames.XAPIKey, out var extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = ContentTypePlainText;
            await context.Response.WriteAsync(HeaderNames.XAPIKey + " is required");
            return;
        }

        var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
        string apiKey = appSettings.GetValue<string>(HeaderNames.XAPIKey);

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = ContentTypePlainText;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await _next(context);
    }
}
