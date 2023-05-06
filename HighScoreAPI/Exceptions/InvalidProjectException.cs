namespace HighScoreAPI.Exceptions;

public class InvalidProjectException : Exception
{
    public InvalidProjectException(string message) : base(message) { }
}
