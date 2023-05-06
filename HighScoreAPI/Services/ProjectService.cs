using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Properties;
using System.Text.RegularExpressions;

namespace HighScoreAPI.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectDataMapper _dataMapper;

    public ProjectService(IProjectDataMapper dataMapper)
    {
        _dataMapper = dataMapper;
    }

    public async Task<Project> GetProjectByNameAsync(string projectName)
    {
        var result = await _dataMapper.GetProjectByNameAsync(projectName);

        if (result is null)
            throw new ProjectNotFoundException(projectName);

        return result;
    }

    public Task AddProjectAsync(Project projectToAdd)
    {
        if (string.IsNullOrWhiteSpace(projectToAdd.Name) || projectToAdd.Name.Length > Constants.ProjectNameMaxLength)
            throw new InvalidProjectException($"The project name must be between 1 and {Constants.ProjectNameMaxLength} characters!");

        projectToAdd.Name = projectToAdd.Name.Trim();
        projectToAdd.Name = Regex.Replace(projectToAdd.Name, @"\s+", "-");

        return _dataMapper.AddProjectAsync(projectToAdd);
    }

    public Task DeleteProjectByNameAsync(string projectName)
    {
        return _dataMapper.DeleteProjectByNameAsync(projectName);
    }
}
