using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HighScoreAPI.Controllers;

[Route("api/highscores")]
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
    [ProducesResponseType(typeof(IEnumerable<HighScore>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<HighScore>>> GetTopAsync(int amount = 10)
    {
        try
        {
            var result = await _service.GetTopAsync(amount);

            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, "Oops! Something went wrong. Try again later.");
        }
    }

    [HttpGet("search/{username}")]
    [ProducesResponseType(typeof(HighScore), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<HighScore>> GetHighScoreByUsernameAsync(string username)
    {
        try
        {
            var result = await _service.GetHighScoreByUsernameAsync(username);

            return Ok(result);
        }
        catch (HighScoreNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, "Oops! Something went wrong. Try again later.");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(HighScore), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddHighScoreAsync(HighScore highScoreToAdd)
    {
        try
        {
            await _service.AddHighScoreAsync(highScoreToAdd);

            return Created($"/api/highscores/search/{highScoreToAdd.Username}", highScoreToAdd);
        }
        catch (InvalidHighScoreException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, "Oops! Something went wrong. Try again later.");
        }
    }

    [HttpDelete]
    [ProducesResponseType(typeof(HighScore), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAllHighScoresAsync()
    {
        try
        {
            await _service.DeleteAllHighScoresAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return StatusCode(500, "Oops! Something went wrong. Try again later.");
        }
    }
}
