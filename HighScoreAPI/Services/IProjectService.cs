using HighScoreAPI.DTOs;
using HighScoreAPI.Models;

namespace HighScoreAPI.Services;

public interface IProjectService
{
    Task<Project> GetProjectByNameAsync(string projectName);
    Task<Project> AddProjectAsync(ProjectDTO projectToAdd);
    Task DeleteProjectByNameAsync(string projectName);
}
