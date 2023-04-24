using HighScoreAPI.Exceptions;

namespace HighScoreAPI.Test.Exceptions;

[TestClass]
public class InvalidHighScoreExceptionTest
{
    [TestMethod]
    public void Constructor_InvalidHighScoreException_ReturnsExpected()
    {
        // Arrange
        string exceptionMessage = "Something went wrong!";

        // Act
        var result = new InvalidHighScoreException(exceptionMessage);

        // Assert
        Assert.AreEqual("Something went wrong!", result.Message);
    }
}
