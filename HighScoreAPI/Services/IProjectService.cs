using HighScoreAPI.Models;

namespace HighScoreAPI.Services;

public interface IProjectService
{
    Task<Project> GetProjectByNameAsync(string projectName);
    Task AddProjectAsync(Project projectToAdd);
    Task DeleteProjectByNameAsync(string projectName);
}
