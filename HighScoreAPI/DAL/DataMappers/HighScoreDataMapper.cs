using HighScoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.DAL.DataMappers;

public class HighScoreDataMapper : IHighScoreDataMapper
{
    private readonly DbContextOptions<HighScoreContext> _options;

    public HighScoreDataMapper(DbContextOptions<HighScoreContext> options)
    {
        _options = options;
    }

    public async Task<IEnumerable<HighScore>> GetTopAsync(int amount)
    {
        using var context = new HighScoreContext(_options);

        var result = await context.HighScores
            .OrderByDescending(hs => hs.Score)
            .Take(amount)
            .ToListAsync();

        return result;
    }

    public async Task<HighScore> GetHighScoreByUsernameAsync(string username)
    {
        using var context = new HighScoreContext(_options);

        var result = await context.HighScores
            .FirstOrDefaultAsync(hs => hs.Username == username);

        return result!;
    }

    public async Task AddHighScoreAsync(HighScore highScoreToAdd)
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

    public async Task DeleteHighScoreAsync(HighScore highScoreToDelete)
    {
        using var context = new HighScoreContext(_options);

        var existingHighScore = await context.HighScores
            .FirstOrDefaultAsync(hs => hs.Username == highScoreToDelete.Username &&
                                       hs.Score == highScoreToDelete.Score);

        if (existingHighScore is null)
            return;

        context.Remove(existingHighScore);

        await context.SaveChangesAsync();
    }

    public async Task DeleteAllHighScoresAsync()
    {
        using var context = new HighScoreContext(_options);

        context.RemoveRange(context.HighScores);

        await context.SaveChangesAsync();
    }
}
