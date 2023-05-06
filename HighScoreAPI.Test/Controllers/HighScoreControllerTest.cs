using HighScoreAPI.Controllers;
using HighScoreAPI.DTOs;
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
    public async Task GetTopAsync_HighScoreController_CallsService()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        int amount = 5;
        var highScores = new List<HighScoreDTO>()
        {
            new("K03N", 423),
            new("Your Partner In Science", 34)
        };
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(highScores);

        // Act
        await _sut.GetTopAsync(projectName, amount);

        // Assert
        _serviceMock.Verify(s => s.GetTopAsync("Smuggling-Pirates", 5), Times.Once);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_CallsServiceWithDefaultAmount10()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScores = new List<HighScoreDTO>()
        {
            new("K03N", 423),
            new("Your Partner In Science", 34)
        };
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(highScores);

        // Act
        await _sut.GetTopAsync(projectName);

        // Assert
        _serviceMock.Verify(s => s.GetTopAsync("Smuggling-Pirates", 10), Times.Once);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScores = new List<HighScoreDTO>()
        {
            new("K03N", 423),
            new("Your Partner In Science", 34)
        };
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ReturnsAsync(highScores);

        // Act
        var result = await _sut.GetTopAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okObjectResult = result.Result as OkObjectResult;
        Assert.IsInstanceOfType(okObjectResult.Value, typeof(IEnumerable<HighScoreDTO>));
        var resultHighScores = okObjectResult.Value as IEnumerable<HighScoreDTO>;
        Assert.AreEqual(2, resultHighScores.Count());
        Assert.IsTrue(resultHighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
        Assert.IsTrue(resultHighScores.Any(hs => hs.Username == "Your Partner In Science" && hs.Score == 34));
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_ServiceThrowsArgumentOutOfRangeException_ReturnsStatusCode400()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException(null, "Something went wrong!"));

        // Act
        var result = await _sut.GetTopAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Something went wrong!", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        int amount = 10;
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.GetTopAsync(projectName, amount);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        int amount = 10;
        _serviceMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.GetTopAsync(projectName, amount);

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
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var highScore = new HighScoreDTO(username, 423);
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(highScore);

        // Act
        await _sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        _serviceMock.Verify(s => s.GetHighScoreByUsernameAsync("Smuggling-Pirates", "K03N"), Times.Once);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var highScore = new HighScoreDTO(username, 423);
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(highScore);

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okObjectResult = result.Result as OkObjectResult;
        Assert.IsInstanceOfType(okObjectResult.Value, typeof(HighScoreDTO));
        var resultHighScore = okObjectResult.Value as HighScoreDTO;
        Assert.AreEqual("K03N", resultHighScore.Username);
        Assert.AreEqual(423, resultHighScore.Score);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ServiceThrowsHighScoreNotFoundException_ReturnsNotFound()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new HighScoreNotFoundException(username));

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        var notFoundObjectResult = result.Result as NotFoundObjectResult;
        Assert.AreEqual("High Score for user with username \"K03N\" could not be found.", notFoundObjectResult.Value);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        _serviceMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(projectName, username);

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
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        _serviceMock.Verify(s => s.AddHighScoreAsync(
            "Smuggling-Pirates",
            It.Is<HighScoreDTO>(hs =>
                hs.Username == "K03N" &&
                hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ReturnsCreated()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(CreatedResult));
        var createdResult = result as CreatedResult;
        Assert.AreEqual("/api/highscores/search/K03N", createdResult.Location);
        Assert.IsInstanceOfType(createdResult.Value, typeof(HighScoreDTO));
        var resultHighScore = createdResult.Value as HighScoreDTO;
        Assert.AreEqual("K03N", resultHighScore.Username);
        Assert.AreEqual(423, resultHighScore.Score);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsInvalidHighScoreException_ReturnsBadRequest()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .ThrowsAsync(new InvalidHighScoreException("Something went wrong!"));

        // Act
        var result = await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestObjectResult = result as ObjectResult;
        Assert.AreEqual("Something went wrong!", badRequestObjectResult.Value);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsProjectNotFoundException_ReturnsBadRequest()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .ThrowsAsync(new ProjectNotFoundException(projectName));

        // Act
        var result = await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestObjectResult = result as ObjectResult;
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", badRequestObjectResult.Value);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScore_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

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
        string projectName = "Smuggling-Pirates";
        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteHighScoreAsync(projectName, highScoreToDelete);

        // Assert
        _serviceMock.Verify(s => s.DeleteHighScoreAsync(
            "Smuggling-Pirates",
            It.Is<HighScoreDTO>(hs =>
                hs.Username == "K03N" &&
                hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteHighScoreAsync(projectName, highScoreToDelete);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreController_ServiceThrowsHighScoreNotFoundException_ReturnsNotFound()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .ThrowsAsync(new HighScoreNotFoundException(highScoreToDelete));

        // Act
        var result = await _sut.DeleteHighScoreAsync(projectName, highScoreToDelete);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var notFoundObjectResult = result as NotFoundObjectResult;
        Assert.AreEqual("High Score for user with username \"K03N\" and score \"423\" could not be found.", notFoundObjectResult.Value);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScore_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _serviceMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.DeleteHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }
}
