namespace HighScoreServer.Exceptions;

public class InvalidHighScoreException : Exception
{
    public InvalidHighScoreException(string message) : base(message) { }
}
