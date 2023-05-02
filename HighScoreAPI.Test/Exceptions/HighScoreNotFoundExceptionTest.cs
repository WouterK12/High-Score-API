using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;

namespace HighScoreAPI.Test.Exceptions;

[TestClass]
public class HighScoreNotFoundExceptionTest
{
    [TestMethod]
    public void Constructor_Username_HighScoreNotFoundException_ReturnsExpected()
    {
        // Arrange
        string username = "K03N";

        // Act
        var result = new HighScoreNotFoundException(username);

        // Assert
        Assert.AreEqual("High Score for user with username \"K03N\" could not be found.", result.Message);
    }

    [TestMethod]
    public void Constructor_HighScore_HighScoreNotFoundException_ReturnsExpected()
    {
        // Arrange
        HighScore highScore = new() { Username = "K03N", Score = 439 };

        // Act
        var result = new HighScoreNotFoundException(highScore);

        // Assert
        Assert.AreEqual("High Score for user with username \"K03N\" and score \"439\" could not be found.", result.Message);
    }
}
