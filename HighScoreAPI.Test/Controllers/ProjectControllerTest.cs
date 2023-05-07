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
public class ProjectControllerTest
{
    private ProjectController _sut = null!;

    private Mock<IProjectService> _serviceMock = null!;
    private Mock<ILogger<ProjectController>> _loggerMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _serviceMock = new Mock<IProjectService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<ProjectController>>();

        _sut = new ProjectController(_serviceMock.Object, _loggerMock.Object);
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectController_CallsService()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var project = new Project() { Name = projectName };
        _serviceMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                    .ReturnsAsync(project);

        // Act
        await _sut.GetProjectByNameAsync(projectName);

        // Assert
        _serviceMock.Verify(s => s.GetProjectByNameAsync("Smuggling-Pirates"), Times.Once);
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectController_ReturnsOk()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        _serviceMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                    .ReturnsAsync(project);

        // Act
        var result = await _sut.GetProjectByNameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okObjectResult = result.Result as OkObjectResult;
        Assert.IsInstanceOfType(okObjectResult.Value, typeof(Project));
        var resultProject = okObjectResult.Value as Project;
        Assert.AreEqual(projectName, resultProject.Name);
        Assert.AreEqual(1, resultProject.HighScores.Count);
        Assert.IsTrue(resultProject.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectController_ServiceThrowsHighScoreNotFoundException_ReturnsNotFound()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ProjectNotFoundException(projectName));

        // Act
        var result = await _sut.GetProjectByNameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        var notFoundObjectResult = result.Result as NotFoundObjectResult;
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", notFoundObjectResult.Value);
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.GetProjectByNameAsync(projectName);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.GetProjectByNameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        var objectResult = result.Result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }

    [TestMethod]
    public async Task AddProjectAsync_Project_ProjectController_CallsService()
    {
        // Arrange
        var projectToAdd = new ProjectDTO("Smuggling-Pirates");
        _serviceMock.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDTO>()))
                    .ReturnsAsync(new Project { Name = "Smuggling-Pirates" });

        // Act
        await _sut.AddProjectAsync(projectToAdd);

        // Assert
        _serviceMock.Verify(s => s.AddProjectAsync(It.Is<ProjectDTO>(p =>
            p.Name == "Smuggling-Pirates")),
            Times.Once);
    }

    [TestMethod]
    public async Task AddProjectAsync_Project_ProjectController_ReturnsCreated()
    {
        // Arrange
        var projectToAdd = new ProjectDTO("Smuggling-Pirates");
        _serviceMock.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDTO>()))
                    .ReturnsAsync(new Project { Name = "Smuggling-Pirates" });

        // Act
        var result = await _sut.AddProjectAsync(projectToAdd);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(CreatedResult));
        var createdResult = result.Result as CreatedResult;
        Assert.AreEqual("/api/projects/search/Smuggling-Pirates", createdResult.Location);
        Assert.IsInstanceOfType(createdResult.Value, typeof(Project));
        var resultProject = createdResult.Value as Project;
        Assert.AreEqual("Smuggling-Pirates", resultProject.Name);
    }

    [TestMethod]
    public async Task AddProjectAsync_Project_ProjectController_ServiceThrowsInvalidProjectException_ReturnsBadRequest()
    {
        // Arrange
        var projectToAdd = new ProjectDTO("Smuggling-Pirates");
        _serviceMock.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDTO>()))
                    .ThrowsAsync(new InvalidProjectException("Something went wrong!"));

        // Act
        var result = await _sut.AddProjectAsync(projectToAdd);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        var badRequestObjectResult = result.Result as ObjectResult;
        Assert.AreEqual("Something went wrong!", badRequestObjectResult.Value);
    }

    [TestMethod]
    public async Task AddProjectAsync_Project_ProjectController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        var projectToAdd = new ProjectDTO("Smuggling-Pirates");
        _serviceMock.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDTO>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.AddProjectAsync(projectToAdd);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task AddProjectAsync_Project_ProjectController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        var projectToAdd = new ProjectDTO("Smuggling-Pirates");
        _serviceMock.Setup(s => s.AddProjectAsync(It.IsAny<ProjectDTO>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.AddProjectAsync(projectToAdd);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        var objectResult = result.Result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }

    [TestMethod]
    public async Task DeleteProjectByNameAsync_HighScoreController_CallsService()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.DeleteProjectByNameAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteProjectByNameAsync(projectName);

        // Assert
        _serviceMock.Verify(s => s.DeleteProjectByNameAsync("Smuggling-Pirates"), Times.Once);
    }

    [TestMethod]
    public async Task DeleteProjectByNameAsync_HighScoreController_ReturnsOk()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.DeleteProjectByNameAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteProjectByNameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkResult));
    }

    [TestMethod]
    public async Task DeleteProjectByNameAsync_HighScoreController_ServiceThrowsException_CallsLogger()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.DeleteProjectByNameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        await _sut.DeleteProjectByNameAsync(projectName);

        // Assert
        _loggerMock.Verify(LogLevel.Error, "Something went wrong!", Times.Once);
    }

    [TestMethod]
    public async Task DeleteProjectByNameAsync_HighScoreController_ServiceThrowsException_ReturnsStatusCode500()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _serviceMock.Setup(s => s.DeleteProjectByNameAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("Something went wrong!"));

        // Act
        var result = await _sut.DeleteProjectByNameAsync(projectName);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        var objectResult = result as ObjectResult;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("Oops! Something went wrong. Try again later.", objectResult.Value);
    }
}
