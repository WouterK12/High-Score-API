using HighScoreAPI.Attributes;
using HighScoreAPI.DTOs;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Properties;
using HighScoreAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HighScoreAPI.Controllers;

[RequiresAdminKey]
[Route("api/projects")]
[ApiController]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _service;
    private readonly ILogger<ProjectController> _logger;

    public ProjectController(IProjectService service, ILogger<ProjectController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("search/{projectName}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Project>> GetProjectByNameAsync(string projectName)
    {
        try
        {
            var result = await _service.GetProjectByNameAsync(projectName);

            return Ok(result);
        }
        catch (ProjectNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, Constants.SomethingWentWrongMessage);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Project>> AddProjectAsync(ProjectDTO projectToAdd)
    {
        try
        {
            var result = await _service.AddProjectAsync(projectToAdd);

            return Created($"/api/projects/search/{projectToAdd.Name}", result);
        }
        catch (InvalidProjectException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, Constants.SomethingWentWrongMessage);
        }
    }

    [HttpDelete("{projectName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteProjectByNameAsync(string projectName)
    {
        try
        {
            await _service.DeleteProjectByNameAsync(projectName);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, Constants.SomethingWentWrongMessage);
        }
    }
}
