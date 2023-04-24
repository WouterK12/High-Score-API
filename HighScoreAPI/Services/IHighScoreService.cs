using HighScoreAPI.Models;

namespace HighScoreAPI.Services
{
    public interface IHighScoreService
    {
        Task<IEnumerable<HighScore>> GetTop(int amount);
        Task<HighScore> GetHighScoreByUsername(string username);
        Task AddHighScore(HighScore highScoreToAdd);
        Task DeleteAllHighScores();
    }
}