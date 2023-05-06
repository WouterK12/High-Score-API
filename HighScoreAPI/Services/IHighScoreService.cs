using HighScoreAPI.DTOs;
using HighScoreAPI.Models;

namespace HighScoreAPI.Services;

public interface IHighScoreService
{
    Task<IEnumerable<HighScoreDTO>> GetTopAsync(string projectName, int amount);
    Task<HighScoreDTO> GetHighScoreByUsernameAsync(string projectName, string username);
    Task AddHighScoreAsync(string projectName, HighScoreDTO highScoreToAdd);
    Task DeleteHighScoreAsync(string projectName, HighScoreDTO highScoreToDelete);
}
