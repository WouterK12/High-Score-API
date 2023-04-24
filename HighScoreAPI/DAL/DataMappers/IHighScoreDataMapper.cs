using HighScoreAPI.Models;

namespace HighScoreAPI.DAL.DataMappers;

public interface IHighScoreDataMapper
{
    Task<IEnumerable<HighScore>> GetTop(int amount);
    Task<HighScore> GetHighScoreByUsername(string username);
    Task AddHighScore(HighScore highScoreToAdd);
    Task DeleteAllHighScores();
}