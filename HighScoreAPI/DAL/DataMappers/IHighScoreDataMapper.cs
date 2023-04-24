using HighScoreAPI.Models;

namespace HighScoreAPI.DAL.DataMappers;

public interface IHighScoreDataMapper
{
    Task<IEnumerable<HighScore>> GetTop10();
    Task<HighScore> GetHighScoreByUsername(string username);
    Task AddHighScore(HighScore highScoreToAdd);
}