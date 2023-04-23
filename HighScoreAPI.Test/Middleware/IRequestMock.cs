using Microsoft.AspNetCore.Http;

namespace HighScoreAPI.Test.Middleware;

public interface IRequestMock
{
    public Task Next(HttpContext context);
}
