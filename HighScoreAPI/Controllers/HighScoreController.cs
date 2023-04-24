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

    [HttpGet("top10")]
    [ProducesResponseType(typeof(IEnumerable<HighScore>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<HighScore>>> GetTop10()
    {
        try
        {
            var result = await _service.GetTop10();

            return Ok(result);
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
    public async Task<ActionResult<HighScore>> GetHighScoreByUsername(string username)
    {
        try
        {
            var result = await _service.GetHighScoreByUsername(username);

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

    [HttpPost("add")]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(HighScore), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> AddHighScore(HighScore highScoreToAdd)
    {
        try
        {
            await _service.AddHighScore(highScoreToAdd);

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
}
