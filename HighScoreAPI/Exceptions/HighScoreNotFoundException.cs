using HighScoreAPI.Models;

namespace HighScoreAPI.Exceptions;

public class HighScoreNotFoundException : Exception
{
	public HighScoreNotFoundException(string username) : base($"High Score for user with username \"{username}\" could not be found.") { }
	public HighScoreNotFoundException(HighScore highScore) : base($"High Score for user with username \"{highScore.Username}\" and score \"{highScore.Score}\" could not be found.") { }
}
