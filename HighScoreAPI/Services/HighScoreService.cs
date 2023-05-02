using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using ProfanityFilter.Interfaces;
using System.Text.RegularExpressions;

namespace HighScoreAPI.Services;

public class HighScoreService : IHighScoreService
{
    private readonly IHighScoreDataMapper _dataMapper;
    private readonly IProfanityFilter _profanityFilter;

    public HighScoreService(IHighScoreDataMapper dataMapper, IProfanityFilter profanityFilter)
    {
        _dataMapper = dataMapper;
        _profanityFilter = profanityFilter;
    }

    public Task<IEnumerable<HighScore>> GetTopAsync(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(null, "The amount must be greater than 0!");

        return _dataMapper.GetTopAsync(amount);
    }

    public async Task<HighScore> GetHighScoreByUsernameAsync(string username)
    {
        var result = await _dataMapper.GetHighScoreByUsernameAsync(username);

        if (result is null)
            throw new HighScoreNotFoundException(username);

        return result;
    }

    public Task AddHighScoreAsync(HighScore highScoreToAdd)
    {
        if (highScoreToAdd.Score <= 0)
            throw new InvalidHighScoreException("Your Score must be greater than 0!");

        if (string.IsNullOrWhiteSpace(highScoreToAdd.Username) || highScoreToAdd.Username.Length > HighScoreProperties.UsernameMaxLength)
            throw new InvalidHighScoreException($"Your Username must be between 1 and {HighScoreProperties.UsernameMaxLength} characters!");

        highScoreToAdd.Username = _profanityFilter.CensorString(highScoreToAdd.Username, ' ');
        highScoreToAdd.Username = Regex.Replace(highScoreToAdd.Username, @"\s+", " ");
        highScoreToAdd.Username = highScoreToAdd.Username.Trim();

        if (string.IsNullOrWhiteSpace(highScoreToAdd.Username))
            throw new InvalidHighScoreException($"Your Username must be between 1 and {HighScoreProperties.UsernameMaxLength} characters!");

        return _dataMapper.AddHighScoreAsync(highScoreToAdd);
    }

    public async Task DeleteHighScoreAsync(HighScore highScoreToDelete)
    {
        var result = await _dataMapper.GetHighScoreByUsernameAsync(highScoreToDelete.Username);

        if (result is null || result.Score != highScoreToDelete.Score)
            throw new HighScoreNotFoundException(highScoreToDelete);

        await _dataMapper.DeleteHighScoreAsync(highScoreToDelete);
    }

    public Task DeleteAllHighScoresAsync()
    {
        return _dataMapper.DeleteAllHighScoresAsync();
    }
}
