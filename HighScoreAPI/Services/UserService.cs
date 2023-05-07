using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Exceptions;
using Syllabore;

namespace HighScoreAPI.Services;

public class UserService : IUserService
{
    private readonly IProjectDataMapper _dataMapper;

    public UserService(IProjectDataMapper dataMapper)
    {
        _dataMapper = dataMapper;
    }

    public async Task<string> GetRandomUsernameAsync(string projectName)
    {
        var existingProject = await _dataMapper.GetProjectByNameAsync(projectName);

        if (existingProject is null)
            throw new ProjectNotFoundException(projectName);

        var generator = new NameGenerator();
        string randomUsername = generator.Next();

        while (existingProject.HighScores.Any(hs => hs.Username == randomUsername))
            randomUsername = generator.Next();

        return randomUsername;
    }
}
