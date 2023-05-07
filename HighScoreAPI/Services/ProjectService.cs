using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.DTOs;
using HighScoreAPI.Encryption;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Properties;
using System.Security.Cryptography;
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

    public async Task<Project> AddProjectAsync(ProjectDTO projectToAdd)
    {
        if (string.IsNullOrWhiteSpace(projectToAdd.Name) || projectToAdd.Name.Length > Constants.ProjectNameMaxLength)
            throw new InvalidProjectException($"The project name must be between 1 and {Constants.ProjectNameMaxLength} characters!");

        string projectName = projectToAdd.Name.Trim();
        projectName = Regex.Replace(projectName, @"\s+", "-");

        string keyBase64 = AesOperation.GenerateKeyBase64();

        Project toAdd = new() { Name = projectName, AesKeyBase64 = keyBase64 };

        await _dataMapper.AddProjectAsync(toAdd);

        return toAdd;
    }

    public Task DeleteProjectByNameAsync(string projectName)
    {
        return _dataMapper.DeleteProjectByNameAsync(projectName);
    }
}
