using HighScoreAPI.Models;

namespace HighScoreAPI.Services
{
    public interface IHighScoreService
    {
        Task<IEnumerable<HighScore>> GetTopAsync(int amount);
        Task<HighScore> GetHighScoreByUsernameAsync(string username);
        Task AddHighScoreAsync(HighScore highScoreToAdd);
        Task DeleteHighScoreAsync(HighScore highScoreToDelete);
        Task DeleteAllHighScoresAsync();
    }
}