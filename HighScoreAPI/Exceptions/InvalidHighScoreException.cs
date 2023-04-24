namespace HighScoreAPI.Exceptions;

public class InvalidHighScoreException : Exception
{
    public InvalidHighScoreException(string message) : base(message) { }
}
