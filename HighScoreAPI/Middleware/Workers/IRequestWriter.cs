namespace HighScoreAPI.Middleware.Workers;

public interface IRequestWriter
{
    Task WriteBadRequestAsync(HttpContext context, string message = "Bad Request");
    Task WriteUnauthorizedAsync(HttpContext context, string message = "Unauthorized");
}
