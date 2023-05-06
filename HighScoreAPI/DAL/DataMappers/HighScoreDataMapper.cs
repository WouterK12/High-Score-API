using HighScoreAPI.DTOs;
using HighScoreAPI.Extensions;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.DAL.DataMappers;

public class HighScoreDataMapper : IHighScoreDataMapper
{
    private readonly DbContextOptions<DatabaseContext> _options;

    public HighScoreDataMapper(DbContextOptions<DatabaseContext> options)
    {
        _options = options;
    }

    public async Task<IEnumerable<HighScoreDTO>> GetTopAsync(string projectName, int amount)
    {
        using var context = new DatabaseContext(_options);

        var result = await context.HighScores
            .Include(hs => hs.Project)
            .Where(hs => hs.Project.Name == projectName)
            .OrderByDescending(hs => hs.Score)
            .Take(amount)
            .Select(hs => new HighScoreDTO(hs.Username, hs.Score))
            .ToListAsync();

        return result;
    }

    public async Task<HighScoreDTO?> GetHighScoreByUsernameAsync(string projectName, string username)
    {
        using var context = new DatabaseContext(_options);

        var result = await context.HighScores
            .Include(hs => hs.Project)
            .Where(hs => hs.Project.Name == projectName)
            .SingleOrDefaultAsync(hs => hs.Username == username);

        if (result is null)
            return null;

        return result.ToDTO();
    }

    public async Task AddHighScoreAsync(string projectName, HighScoreDTO highScoreToAdd)
    {
        using var context = new DatabaseContext(_options);

        var existingProject = await context.Projects
            .Include(p => p.HighScores)
            .SingleAsync(p => p.Name == projectName);

        var existingHighScore = existingProject.HighScores
            .FirstOrDefault(hs => hs.Username == highScoreToAdd.Username);

        if (existingHighScore is null)
        {
            var toAdd = highScoreToAdd.ToHighScore();
            toAdd.Project = existingProject;
            await context.AddAsync(toAdd);
        }
        else if (highScoreToAdd.Score > existingHighScore.Score)
        {
            existingHighScore.Score = highScoreToAdd.Score;
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteHighScoreAsync(string projectName, HighScoreDTO highScoreToDelete)
    {
        using var context = new DatabaseContext(_options);

        var existingHighScore = await context.HighScores
            .Include(hs => hs.Project)
            .Where(hs => hs.Project.Name == projectName)
            .FirstOrDefaultAsync(hs => hs.Username == highScoreToDelete.Username &&
                                       hs.Score == highScoreToDelete.Score);

        if (existingHighScore is null)
            return;

        context.Remove(existingHighScore);

        await context.SaveChangesAsync();
    }
}
