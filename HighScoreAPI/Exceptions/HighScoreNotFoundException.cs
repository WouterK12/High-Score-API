namespace HighScoreAPI.Exceptions;

public class HighScoreNotFoundException : Exception
{
	public HighScoreNotFoundException(string username) : base($"High Score for user with username \"{username}\" could not be found.") { }
}
