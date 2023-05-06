using HighScoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.DAL.DataMappers;

public class ProjectDataMapper : IProjectDataMapper
{
    private readonly DbContextOptions<DatabaseContext> _options;

    public ProjectDataMapper(DbContextOptions<DatabaseContext> options)
    {
        _options = options;
    }

    public async Task<Project?> GetProjectByNameAsync(string projectName)
    {
        using var context = new DatabaseContext(_options);

        var result = await context.Projects
            .Include(p => p.HighScores)
            .SingleOrDefaultAsync(p => p.Name == projectName);

        return result;
    }

    public async Task AddProjectAsync(Project projectToAdd)
    {
        using var context = new DatabaseContext(_options);

        var existingProject = await context.Projects
            .Include(p => p.HighScores)
            .SingleOrDefaultAsync(p => p.Name == projectToAdd.Name);

        if (existingProject is not null)
            return;

        await context.AddAsync(projectToAdd);
        await context.SaveChangesAsync();
    }

    public async Task DeleteProjectByNameAsync(string projectName)
    {
        using var context = new DatabaseContext(_options);

        var project = await context.Projects
            .FirstOrDefaultAsync(p => p.Name == projectName);

        if (project is null)
            return;

        context.Remove(project);

        await context.SaveChangesAsync();
    }
}
