using HighScoreAPI.DTOs;

namespace HighScoreAPI.Exceptions;

public class HighScoreNotFoundException : Exception
{
	public HighScoreNotFoundException(string username) : base($"High Score for user with username \"{username}\" could not be found.") { }
	public HighScoreNotFoundException(HighScoreDTO highScoreDTO) : base($"High Score for user with username \"{highScoreDTO.Username}\" and score \"{highScoreDTO.Score}\" could not be found.") { }
}
