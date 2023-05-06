using HighScoreAPI.DTOs;
using HighScoreAPI.Extensions;
using HighScoreAPI.Models;

namespace HighScoreAPI.Test.Extensions;

[TestClass]
public class HighScoreExtensionsTest
{
    [TestMethod]
    public void ToHighScore_HighScoreExtensions_ReturnsHighScore()
    {
        // Arrange
        var highScoreDTO = new HighScoreDTO("K03N", 423);

        // Act
        var result = HighScoreExtensions.ToHighScore(highScoreDTO);

        // Assert
        Assert.AreEqual("K03N", result.Username);
        Assert.AreEqual(423, result.Score);
    }

    [TestMethod]
    public void ToDTO_HighScoreExtensions_ReturnsDTO()
    {
        // Arrange
        var highScore = new HighScore() { Username = "K03N", Score = 423 };

        // Act
        var result = HighScoreExtensions.ToDTO(highScore);

        // Assert
        Assert.AreEqual("K03N", result.Username);
        Assert.AreEqual(423, result.Score);
    }
}
