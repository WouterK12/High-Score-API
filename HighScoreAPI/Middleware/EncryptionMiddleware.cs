using HighScoreAPI.Attributes;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Encryption;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Extensions;
using HighScoreAPI.Middleware.Workers;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Newtonsoft.Json;
using System.Text;

namespace HighScoreAPI.Middleware;

public class EncryptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRequestWriter _requestWriter;
    private readonly IProjectDataMapper _projectDataMapper;

    private const string ProjectNameRouteValueName = "projectName";

    private const string RequestCouldNotBeDeserializedMessage = "The requested body could not be deserialized to the desired type.";
    private const string RequestCouldNotBeDecryptedMessage = "The requested body could not be decrypted.";

    public EncryptionMiddleware(RequestDelegate next, IRequestWriter requestWriter, IProjectDataMapper projectDataMapper)
    {
        _next = next;
        _requestWriter = requestWriter;
        _projectDataMapper = projectDataMapper;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var endpointFeature = context.Features.Get<IEndpointFeature>();
        var endpoint = endpointFeature?.Endpoint;

        if (endpoint is null)
            return _next(context);

        bool hasRequiresEncryptedBodyAttribute = endpoint.Metadata.Any(m => m is RequiresEncryptedBodyAttribute);

        if (!hasRequiresEncryptedBodyAttribute)
            return _next(context);

        bool hasAesVectorHeader = context.Request.Headers.TryGetValue(HeaderNames.AESVector, out var aesVectorBase64);

        if (!hasAesVectorHeader)
            return _requestWriter.WriteBadRequestAsync(context, HeaderNames.AESVector + " is required");

        return TryDecryptBodyAsync(context, endpoint, aesVectorBase64);
    }

    private async Task TryDecryptBodyAsync(HttpContext context, Endpoint endpoint, string aesVectorBase64)
    {
        try
        {
            string keyBase64 = await GetProjectEncryptionKeyBase64(context.Request.RouteValues);
            string jsonString = await DecryptJsonStringAsync(context.Request.Body, keyBase64, aesVectorBase64);

            ActionDescriptor descriptor = (ActionDescriptor)endpoint.Metadata.Single(m => m is ActionDescriptor);
            jsonString.DeserializeJsonToBodyType(descriptor, context.Request.RouteValues);

            using MemoryStream stream = new(Encoding.ASCII.GetBytes(jsonString));
            context.Request.Body = stream;

            await _next(context);
        }
        catch (Exception ex)
        {
            string message = RequestCouldNotBeDecryptedMessage;

            if (ex is JsonException)
                message = RequestCouldNotBeDeserializedMessage;
            else if (ex is ArgumentNullException || ex is ProjectNotFoundException)
                message = ex.Message;

            await _requestWriter.WriteBadRequestAsync(context, message);
        }
    }

    private async Task<string> GetProjectEncryptionKeyBase64(RouteValueDictionary routeValues)
    {
        if (!routeValues.TryGetValue(ProjectNameRouteValueName, out object? valueObject) || string.IsNullOrWhiteSpace(valueObject as string))
            throw new ArgumentNullException(null, $"Route Value \"{ProjectNameRouteValueName}\" must be set.");

        string? projectName = valueObject as string;

        var existingProject = await _projectDataMapper.GetProjectByNameAsync(projectName);

        if (existingProject is null)
            throw new ProjectNotFoundException(projectName);

        return existingProject.AesKeyBase64;
    }

    private async Task<string> DecryptJsonStringAsync(Stream body, string keybase64, string aesVectorBase64)
    {
        using StreamReader reader = new(body);
        string bodyString = await reader.ReadToEndAsync();

        return AesOperation.DecryptData(bodyString, keybase64, aesVectorBase64);
    }
}
