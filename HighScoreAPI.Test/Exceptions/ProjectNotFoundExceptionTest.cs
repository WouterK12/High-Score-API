using HighScoreAPI.Exceptions;

namespace HighScoreAPI.Test.Exceptions;

[TestClass]
public class ProjectNotFoundExceptionTest
{
	[TestMethod]
	public void Constructor_ProjectNotFoundException_ReturnsExpected()
	{
        // Arrange
        string projectName = "Smuggling-Pirates";

        // Act
        var result = new ProjectNotFoundException(projectName);

        // Assert
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", result.Message);
    }
}
