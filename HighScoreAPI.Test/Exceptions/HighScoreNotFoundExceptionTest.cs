using HighScoreAPI.Exceptions;

namespace HighScoreAPI.Test.Exceptions;

[TestClass]
public class HighScoreNotFoundExceptionTest
{
    [TestMethod]
    public void Constructor_HighScoreNotFoundException_ReturnsExpected()
    {
        // Arrange
        string username = "K03N";

        // Act
        var result = new HighScoreNotFoundException(username);

        // Assert
        Assert.AreEqual("High Score for user with username \"K03N\" could not be found.", result.Message);
    }
}
