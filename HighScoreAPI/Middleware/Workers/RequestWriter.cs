namespace HighScoreAPI.Middleware.Workers;

public class RequestWriter : IRequestWriter
{
    private const string ContentTypePlainText = "text/plain";

    public Task WriteBadRequestAsync(HttpContext context, string message = "Bad Request")
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = ContentTypePlainText;
        // Stryker disable once all
        return context.Response.WriteAsync(message);
    }

    public Task WriteUnauthorizedAsync(HttpContext context, string message = "Unauthorized")
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = ContentTypePlainText;
        // Stryker disable once all
        return context.Response.WriteAsync(message);
    }
}
