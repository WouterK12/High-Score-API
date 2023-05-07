using HighScoreAPI.Attributes;
using HighScoreAPI.Middleware.Workers;
using Microsoft.AspNetCore.Http.Features;

namespace HighScoreAPI.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRequestWriter _requestWriter;
    private readonly string _clientApiKey;
    private readonly string _adminApiKey;

    private const string ClientConfigurationValueName = "Client";
    private const string AdminConfigurationValueName = "Admin";

    public ApiKeyMiddleware(RequestDelegate next, IRequestWriter requestWriter, IConfiguration configuration)
    {
        _next = next;
        _requestWriter = requestWriter;

        var apiKeys = configuration.GetSection(HeaderNames.XAPIKey);
        _clientApiKey = apiKeys.GetValue<string>(ClientConfigurationValueName);
        _adminApiKey = apiKeys.GetValue<string>(AdminConfigurationValueName);
    }

    public Task InvokeAsync(HttpContext context)
    {
        bool hasApiKeyHeader = context.Request.Headers.TryGetValue(HeaderNames.XAPIKey, out var extractedApiKey);

        if (!hasApiKeyHeader)
        {
            return _requestWriter.WriteBadRequestAsync(context, HeaderNames.XAPIKey + " is required");
        }

        var endpointFeature = context.Features.Get<IEndpointFeature>();
        var endpoint = endpointFeature?.Endpoint;

        if (endpoint is not null)
        {
            bool hasRequiresAdminKeyAttribute = endpoint.Metadata.Any(m => m is RequiresAdminKeyAttribute);

            if (hasRequiresAdminKeyAttribute && !extractedApiKey.Equals(_adminApiKey))
            {
                return _requestWriter.WriteUnauthorizedAsync(context);
            }
        }

        if (!extractedApiKey.Equals(_clientApiKey) && !extractedApiKey.Equals(_adminApiKey))
        {
            return _requestWriter.WriteUnauthorizedAsync(context);
        }

        return _next(context);
    }
}
