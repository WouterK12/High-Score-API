using HighScoreAPI.DTOs;

namespace HighScoreAPI.DAL.DataMappers;

public interface IHighScoreDataMapper
{
    Task<IEnumerable<HighScoreDTO>> GetTopAsync(string projectName, int amount);
    Task<HighScoreDTO?> GetHighScoreByUsernameAsync(string projectName, string username);
    Task AddHighScoreAsync(string projectName, HighScoreDTO highScoreToAdd);
    Task DeleteHighScoreAsync(string projectName, HighScoreDTO highScoreToDelete);
}
