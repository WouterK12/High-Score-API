using HighScoreServer.Models;

namespace HighScoreServer.DAL.DataMappers;

public interface IHighScoreDataMapper
{
    Task<IEnumerable<HighScore>> GetTop10();
    Task<HighScore> GetHighScoreByUsername(string username);
    Task AddHighScore(HighScore highScoreToAdd);
}