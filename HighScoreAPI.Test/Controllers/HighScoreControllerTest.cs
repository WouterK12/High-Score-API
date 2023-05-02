using HighScoreAPI.Controllers;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Services;
using HighScoreAPI.Test.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HighScoreAPI.Test.Controllers;

[TestClass]
public class HighScoreControllerTest
{
    private HighScoreController _sut = null!;

    private Mock<IHighScoreService> _serviceMock = null!;
    private Mock<ILogger<HighScoreController>> _loggerMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _serviceMock = new Mock<IHighScoreService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<HighScoreController>>();

        _sut = new HighScoreController(_serviceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetTopAsync_5_HighScoreController_CallsService()
    {
        // Arrange
        int amount = 5;
        var highScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 }
        };
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<int>()))
                    .ReturnsAsync(highScores);

        // Act
        await _sut.GetTopAsync(amount);

        // Assert
        _serviceMock.Verify(s => s.GetTopAsync(5), Times.Once);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_CallsServiceWithDefaultAmount10()
    {
        // Arrange
        var highScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 }
        };
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<int>()))
                    .ReturnsAsync(highScores);

        // Act
        await _sut.GetTopAsync();

        // Assert
        _serviceMock.Verify(s => s.GetTopAsync(10), Times.Once);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        var highScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 }
        };
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<int>()))
                    .ReturnsAsync(highScores);

        // Act
        var result = await _sut.GetTopAsync();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okObjectResult = result.Result as OkObjectResult;
        Assert.IsInstanceOfType(okObjectResult.Value, typeof(IEnumerable<HighScore>));
        var resultHighScores = okObjectResult.Value as IEnumerable<HighScore>;
        Assert.AreEqual(2, resultHighScores.Count());
        Assert.IsTrue(resultHighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
        Assert.IsTrue(resultHighScores.Any(hs => hs.Username == "Your Partner In Science" && hs.Score == 34));
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_ServiceArgumentOutOfRangeException_ReturnsStatusCode400()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<int>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException(null, "Something went wrong!"));

        // Act
        var result = await _sut.GetTopAsync();

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Something went wrong!", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetTopAsync_10_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        int amount = 10;
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.GetTopAsync(amount);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task GetTopAsync_10_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        int amount = 10;
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.GetTopAsync(amount);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        var objectResult = result.Result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_CallsService()
    {
        // Arrange
        var username = "K03N";
        var highScore = new HighScore() { Username = username, Score = 423 };
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>()))
                    .ReturnsAsync(highScore);

        // Act
        await _sut.GetHighScoreByUsernameAsync(username);

        // Assert
        _serviceMock.Verify(s => s.GetHighScoreByUsernameAsync("K03N"), Times.Once);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        var username = "K03N";
        var highScore = new HighScore() { Username = username, Score = 423 };
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>()))
                    .ReturnsAsync(highScore);

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(username);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okObjectResult = result.Result as OkObjectResult;
        Assert.IsInstanceOfType(okObjectResult.Value, typeof(HighScore));
        var resultHighScore = okObjectResult.Value as HighScore;
        Assert.AreEqual("K03N", resultHighScore.Username);
        Assert.AreEqual(423, resultHighScore.Score);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ServiceThrowsHighScoreNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var username = "K03N";
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new HighScoreNotFoundException(username));

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(username);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        var notFoundObjectResult = result.Result as NotFoundObjectResult;
        Assert.AreEqual("High Score for user with username \"K03N\" could not be found.", notFoundObjectResult.Value);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        var username = "K03N";
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.GetHighScoreByUsernameAsync(username);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        var username = "K03N";
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(username);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        var objectResult = result.Result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_CallsService()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<HighScore>()))
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.AddHighScoreAsync(highScoreToAdd);

        // Assert
        _serviceMock.Verify(s => s.AddHighScoreAsync(It.Is<HighScore>(hs =>
            hs.Username == "K03N" &&
            hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ReturnsCreated()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<HighScore>()))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddHighScoreAsync(highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(CreatedResult));
        var createdResult = result as CreatedResult;
        Assert.AreEqual("/api/highscores/search/K03N", createdResult.Location);
        Assert.IsInstanceOfType(createdResult.Value, typeof(HighScore));
        var resultHighScore = createdResult.Value as HighScore;
        Assert.AreEqual("K03N", resultHighScore.Username);
        Assert.AreEqual(423, resultHighScore.Score);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsInvalidHighScoreException_ReturnsBadRequest()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<HighScore>()))
                    .ThrowsAsync(new InvalidHighScoreException("Something went wrong!"));

        // Act
        var result = await _sut.AddHighScoreAsync(highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestObjectResult = result as ObjectResult;
        Assert.AreEqual("Something went wrong!", badRequestObjectResult.Value);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<HighScore>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.AddHighScoreAsync(highScoreToAdd);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<HighScore>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.AddHighScoreAsync(highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreController_CallsService()
    {
        // Arrange
        var highScoreToDelete = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<HighScore>()))
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteHighScoreAsync(highScoreToDelete);

        // Assert
        _serviceMock.Verify(s => s.DeleteHighScoreAsync(It.Is<HighScore>(hs =>
            hs.Username == "K03N" &&
            hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        var highScoreToDelete = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<HighScore>()))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteHighScoreAsync(highScoreToDelete);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreController_ServiceThrowsHighScoreNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var highScoreToDelete = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<HighScore>()))
                    .ThrowsAsync(new HighScoreNotFoundException(highScoreToDelete));

        // Act
        var result = await _sut.DeleteHighScoreAsync(highScoreToDelete);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.AreEqual("High Score for user with username \"K03N\" and score \"423\" could not be found.", notFoundObjectResult.Value);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScore_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<HighScore>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.DeleteHighScoreAsync(highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }

    [TestMethod]
    public async Task DeleteAllHighScoresAsync_HighScoreController_CallsService()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAllHighScoresAsync())
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAllHighScoresAsync();

        // Assert
        _serviceMock.Verify(s => s.DeleteAllHighScoresAsync(), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAllHighScoresAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAllHighScoresAsync())
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteAllHighScoresAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task DeleteAllHighScoresAsync_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAllHighScoresAsync())
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.DeleteAllHighScoresAsync();

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task DeleteAllHighScoresAsync_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAllHighScoresAsync())
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.DeleteAllHighScoresAsync();

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }
}
