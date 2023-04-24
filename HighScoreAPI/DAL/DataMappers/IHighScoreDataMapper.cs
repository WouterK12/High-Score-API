using HighScoreAPI.Models;

namespace HighScoreAPI.DAL.DataMappers;

public interface IHighScoreDataMapper
{
    Task<IEnumerable<HighScore>> GetTopAsync(int amount);
    Task<HighScore> GetHighScoreByUsernameAsync(string username);
    Task AddHighScoreAsync(HighScore highScoreToAdd);
    Task DeleteAllHighScoresAsync();
}