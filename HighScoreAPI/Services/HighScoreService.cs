using HighScoreAPI.DAL;
using HighScoreServer.DAL.DataMappers;
using HighScoreServer.Exceptions;
using HighScoreServer.Models;

namespace HighScoreServer.Services;

public class HighScoreService : IHighScoreService
{
    private readonly IHighScoreDataMapper _dataMapper;

    public HighScoreService(IHighScoreDataMapper dataMapper)
    {
        _dataMapper = dataMapper;
    }

    public Task<IEnumerable<HighScore>> GetTop10()
    {
        return _dataMapper.GetTop10();
    }

    public async Task<HighScore> GetHighScoreByUsername(string username)
    {
        var result = await _dataMapper.GetHighScoreByUsername(username);

        if (result is null)
            throw new HighScoreNotFoundException(username);

        return result;
    }

    public Task AddHighScore(HighScore highScoreToAdd)
    {
        if (string.IsNullOrWhiteSpace(highScoreToAdd.Username) || highScoreToAdd.Username.Length > HighScoreProperties.UsernameMaxLength)
            throw new InvalidHighScoreException($"Your Username must be between 1 and {HighScoreProperties.UsernameMaxLength} characters!");

        if (highScoreToAdd.Score <= 0)
            throw new InvalidHighScoreException("Your Score must be greater than 0!");

        return _dataMapper.AddHighScore(highScoreToAdd);
    }
}
