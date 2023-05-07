using HighScoreAPI.Controllers;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Services;
using HighScoreAPI.Test.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace HighScoreAPI.Test.Controllers;

[TestClass]
public class UserControllerTest
{
    private UserController _sut = null!;

    private Mock<IUserService> _serviceMock = null!;
    private Mock<ILogger<UserController>> _loggerMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _serviceMock = new Mock<IUserService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<UserController>>();

        _sut = new UserController(_serviceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserController_CallsService()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetRandomUsernameAsync(It.IsAny<string>()))
                    .ReturnsAsync("K03N");

        // Act
        await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        _serviceMock.Verify(s => s.GetRandomUsernameAsync("Smuggling-Pirates"), Times.Once);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserController_ReturnsOk()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetRandomUsernameAsync(It.IsAny<string>()))
                    .ReturnsAsync("K03N");

        // Act
        var result = await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okObjectResult = result.Result as OkObjectResult;
        Assert.IsInstanceOfType(okObjectResult.Value, typeof(string));
        var resultString = okObjectResult.Value as string;
        Assert.AreEqual("K03N", resultString);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserController_ServiceThrowsProjectNotFoundException_ReturnsStatusCode400()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetRandomUsernameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ProjectNotFoundException(projectName));

        // Act
        var result = await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", badRequestResult.Value);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetRandomUsernameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetRandomUsernameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        var objectResult = result.Result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }
}
