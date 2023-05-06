using HighScoreAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;

namespace HighScoreAPI.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _clientApiKey;
    private readonly string _adminApiKey;

    private const string ContentTypePlainText = "text/plain";

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var apiKeys = configuration.GetSection(HeaderNames.XAPIKey);
        _clientApiKey = apiKeys.GetValue<string>("Client");
        _adminApiKey = apiKeys.GetValue<string>("Admin");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        bool hasApiKeyHeader = context.Request.Headers.TryGetValue(HeaderNames.XAPIKey, out var extractedApiKey);

        if (!hasApiKeyHeader)
        {
            await WriteBadRequest(context);
            return;
        }

        var endpointFeature = context.Features.Get<IEndpointFeature>();
        var endpoint = endpointFeature?.Endpoint;

        if (endpoint is not null)
        {
            bool hasRequiresAdminKeyAttribute = endpoint.Metadata.Any(m => m is RequiresAdminKeyAttribute);

            if (hasRequiresAdminKeyAttribute && !extractedApiKey.Equals(_adminApiKey))
            {
                await WriteUnauthorized(context);
                return;
            }
        }

        if (!extractedApiKey.Equals(_clientApiKey) && !extractedApiKey.Equals(_adminApiKey))
        {
            await WriteUnauthorized(context);
            return;
        }

        await _next(context);
    }

    private async Task WriteBadRequest(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = ContentTypePlainText;
        await context.Response.WriteAsync(HeaderNames.XAPIKey + " is required");
    }

    private async Task WriteUnauthorized(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = ContentTypePlainText;
        await context.Response.WriteAsync("Unauthorized");
    }
}
