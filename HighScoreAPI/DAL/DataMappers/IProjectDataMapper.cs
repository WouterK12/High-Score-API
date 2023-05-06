using HighScoreAPI.Models;

namespace HighScoreAPI.DAL.DataMappers;

public interface IProjectDataMapper
{
    Task<Project?> GetProjectByNameAsync(string projectName);
    Task AddProjectAsync(Project projectToAdd);
    Task DeleteProjectByNameAsync(string projectName);
}
