using HighScoreAPI.Exceptions;

namespace HighScoreAPI.Test.Exceptions;

[TestClass]
public class InvalidProjectExceptionTest
{
    [TestMethod]
    public void Constructor_InvalidProjectException_ReturnsExpected()
    {
        // Arrange
        string exceptionMessage = "Something went wrong!";

        // Act
        var result = new InvalidProjectException(exceptionMessage);

        // Assert
        Assert.AreEqual("Something went wrong!", result.Message);
    }
}
