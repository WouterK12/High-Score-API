using HighScoreAPI.Attributes;
using HighScoreAPI.DTOs;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Properties;
using HighScoreAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HighScoreAPI.Controllers;

[Route("api/highscores/{projectName}")]
[ApiController]
public class HighScoreController : ControllerBase
{
    private readonly IHighScoreService _service;
    private readonly ILogger<HighScoreController> _logger;

    public HighScoreController(IHighScoreService service, ILogger<HighScoreController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("top/{amount}")]
    [ProducesResponseType(typeof(IEnumerable<HighScoreDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<HighScoreDTO>>> GetTopAsync(string projectName, int amount = 10)
    {
        try
        {
            var result = await _service.GetTopAsync(projectName, amount);

            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, Constants.SomethingWentWrongMessage);
        }
    }

    [HttpGet("search/{username}")]
    [ProducesResponseType(typeof(HighScoreDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HighScoreDTO>> GetHighScoreByUsernameAsync(string projectName, string username)
    {
        try
        {
            var result = await _service.GetHighScoreByUsernameAsync(projectName, username);

            return Ok(result);
        }
        catch (HighScoreNotFoundException ex)
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
    [ProducesResponseType(typeof(HighScoreDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddHighScoreAsync(string projectName, HighScoreDTO highScoreToAdd)
    {
        try
        {
            await _service.AddHighScoreAsync(projectName, highScoreToAdd);

            return Created($"/api/highscores/search/{highScoreToAdd.Username}", highScoreToAdd);
        }
        catch (InvalidHighScoreException ex)
        {
            return BadRequest(ex.Message);
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

    [RequiresAdminKey]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteHighScoreAsync(string projectName, HighScoreDTO highScoreToDelete)
    {
        try
        {
            await _service.DeleteHighScoreAsync(projectName, highScoreToDelete);

            return Ok();
        }
        catch (HighScoreNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, Constants.SomethingWentWrongMessage);
        }
    }
}
