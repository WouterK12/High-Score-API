using HighScoreServer.Models;
using Microsoft.EntityFrameworkCore;

namespace HighScoreServer.DAL.DataMappers;

public class HighScoreDataMapper : IHighScoreDataMapper
{
    private readonly DbContextOptions<HighScoreContext> _options;

    public HighScoreDataMapper(DbContextOptions<HighScoreContext> options)
    {
        _options = options;
    }

    public async Task<IEnumerable<HighScore>> GetTop10()
    {
        using var context = new HighScoreContext(_options);

        var result = await context.HighScores
            .OrderByDescending(hs => hs.Score)
            .Take(10)
            .ToListAsync();

        return result;
    }

    public async Task<HighScore> GetHighScoreByUsername(string username)
    {
        using var context = new HighScoreContext(_options);

        var result = await context.HighScores
            .FirstOrDefaultAsync(hs => hs.Username == username);

        return result!;
    }

    public async Task AddHighScore(HighScore highScoreToAdd)
    {
        using var context = new HighScoreContext(_options);

        var existingHighScore = await context.HighScores
            .FirstOrDefaultAsync(hs => hs.Username == highScoreToAdd.Username);

        if (existingHighScore is null)
        {
            await context.HighScores.AddAsync(highScoreToAdd);
        }
        else if (highScoreToAdd.Score > existingHighScore.Score)
        {
            existingHighScore.Score = highScoreToAdd.Score;
        }

        await context.SaveChangesAsync();
    }
}
