namespace HighScoreAPI.Exceptions;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException(string projectName) : base($"Project with name \"{projectName}\" could not be found.") { }
}
