using HighScoreAPI.Exceptions;
using HighScoreAPI.Properties;
using HighScoreAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HighScoreAPI.Controllers;

[Route("api/users/{projectName}")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("random")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetRandomUsernameAsync(string projectName)
    {
        try
        {
            var result = await _service.GetRandomUsernameAsync(projectName);

            return Ok(result);
        }
        catch (ProjectNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, Constants.SomethingWentWrongMessage);
        }
    }
}
