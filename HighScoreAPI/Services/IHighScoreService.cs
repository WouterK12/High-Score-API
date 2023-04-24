using HighScoreAPI.Models;

namespace HighScoreAPI.Services
{
    public interface IHighScoreService
    {
        Task<IEnumerable<HighScore>> GetTop10();
        Task<HighScore> GetHighScoreByUsername(string username);
        Task AddHighScore(HighScore highScoreToAdd);
    }
}