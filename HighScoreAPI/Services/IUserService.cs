namespace HighScoreAPI.Services;

public interface IUserService
{
    Task<string> GetRandomUsernameAsync(string projectName);
}
