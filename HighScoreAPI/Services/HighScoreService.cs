using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.DTOs;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Properties;
using ProfanityFilter.Interfaces;
using System.Text.RegularExpressions;

namespace HighScoreAPI.Services;

public class HighScoreService : IHighScoreService
{
    private readonly IHighScoreDataMapper _highScoreDataMapper;
    private readonly IProjectDataMapper _projectDataMapper;
    private readonly IProfanityFilter _profanityFilter;

    public HighScoreService(IHighScoreDataMapper highScoreDataMapper, IProjectDataMapper projectDataMapper, IProfanityFilter profanityFilter)
    {
        _highScoreDataMapper = highScoreDataMapper;
        _projectDataMapper = projectDataMapper;
        _profanityFilter = profanityFilter;
    }

    public Task<IEnumerable<HighScoreDTO>> GetTopAsync(string projectName, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(null, "The amount must be greater than 0!");

        return _highScoreDataMapper.GetTopAsync(projectName, amount);
    }

    public async Task<HighScoreDTO> GetHighScoreByUsernameAsync(string projectName, string username)
    {
        var result = await _highScoreDataMapper.GetHighScoreByUsernameAsync(projectName, username);

        if (result is null)
            throw new HighScoreNotFoundException(username);

        return result;
    }

    public async Task AddHighScoreAsync(string projectName, HighScoreDTO highScoreToAdd)
    {
        if (highScoreToAdd.Score <= 0)
            throw new InvalidHighScoreException("Your Score must be greater than 0!");

        if (string.IsNullOrWhiteSpace(highScoreToAdd.Username) || highScoreToAdd.Username.Length > Constants.UsernameMaxLength)
            throw new InvalidHighScoreException($"Your Username must be between 1 and {Constants.UsernameMaxLength} characters!");

        string username = _profanityFilter.CensorString(highScoreToAdd.Username, ' ');
        username = Regex.Replace(username, @"\s+", " ");
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidHighScoreException($"Your Username must be between 1 and {Constants.UsernameMaxLength} characters!");

        var existingProject = await _projectDataMapper.GetProjectByNameAsync(projectName);

        if (existingProject is null)
            throw new ProjectNotFoundException(projectName);

        highScoreToAdd = new HighScoreDTO(username, highScoreToAdd.Score);

        await _highScoreDataMapper.AddHighScoreAsync(projectName, highScoreToAdd);
    }

    public async Task DeleteHighScoreAsync(string projectName, HighScoreDTO highScoreToDelete)
    {
        var result = await _highScoreDataMapper.GetHighScoreByUsernameAsync(projectName, highScoreToDelete.Username);

        if (result is null || result.Score != highScoreToDelete.Score)
            throw new HighScoreNotFoundException(highScoreToDelete);

        await _highScoreDataMapper.DeleteHighScoreAsync(projectName, highScoreToDelete);
    }
}
