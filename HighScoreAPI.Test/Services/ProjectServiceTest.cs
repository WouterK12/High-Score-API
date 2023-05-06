using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Services;
using Moq;

namespace HighScoreAPI.Test.Services;

[TestClass]
public class ProjectServiceTest
{
    private ProjectService _sut = null!;

    private Mock<IProjectDataMapper> _dataMapperMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _dataMapperMock = new Mock<IProjectDataMapper>(MockBehavior.Strict);

        _sut = new ProjectService(_dataMapperMock.Object);
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        _dataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                       .ReturnsAsync(project);

        // Act
        var result = await _sut.GetProjectByNameAsync(projectName);

        // Assert
        _dataMapperMock.Verify(dm => dm.GetProjectByNameAsync("Smuggling-Pirates"), Times.Once);
        Assert.AreEqual("Smuggling-Pirates", result.Name);
        Assert.AreEqual(1, result.HighScores.Count);
        Assert.IsTrue(result.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectService_DataMapperReturnsNull_ThrowsProjectNotFoundException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _dataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
                       .ReturnsAsync(() => null!);

        // Act
        async Task<Project> Act()
        {
            return await _sut.GetProjectByNameAsync(projectName);
        }

        // Assert
        ProjectNotFoundException ex = await Assert.ThrowsExceptionAsync<ProjectNotFoundException>(Act);
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", ex.Message);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("AnExtremelyMegaSuperLongProjectNameWithTotalLengthOf65Characters!")]
    public async Task AddProjectAsync_NameNullOrWhiteSpaceOrLongerThan64_ProjectService_ThrowsInvalidProjectException(string projectName)
    {
        // Arrange
        var projectToAdd = new Project { Name = projectName };
        _dataMapperMock.Setup(s => s.AddProjectAsync(It.IsAny<Project>()))
                       .Returns(Task.CompletedTask);

        // Act
        async Task Act()
        {
            await _sut.AddProjectAsync(projectToAdd);
        }

        // Assert
        InvalidProjectException ex = await Assert.ThrowsExceptionAsync<InvalidProjectException>(Act);
        Assert.AreEqual("The project name must be between 1 and 64 characters!", ex.Message);
    }

    [TestMethod]
    public async Task AddProjectAsync_NameLength64_ProjectService_DoesNotThrowException()
    {
        // Arrange
        var nameWithLength64 = "OhAnExtremelyMegaSuperLongProjectNameWithTotalLength64Characters";
        var projectToAdd = new Project { Name = nameWithLength64 };
        _dataMapperMock.Setup(s => s.AddProjectAsync(It.IsAny<Project>()))
                       .Returns(Task.CompletedTask);

        try
        {
            // Act
            await _sut.AddProjectAsync(projectToAdd);
        }
        catch (Exception)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
    }

    [DataTestMethod]
    [DataRow(" SmugglingPirates", "SmugglingPirates")]
    [DataRow("  SmugglingPirates", "SmugglingPirates")]
    [DataRow("Smuggling   Pirates", "Smuggling-Pirates")]
    [DataRow("SmugglingPirates ", "SmugglingPirates")]
    [DataRow("SmugglingPirates  ", "SmugglingPirates")]
    [DataRow(" SmugglingPirates ", "SmugglingPirates")]
    [DataRow("  SmugglingPirates  ", "SmugglingPirates")]
    public async Task AddProjectAsync_NameWithWhiteSpaces_ProjectService_ReplacesWithDashes_CallsDataMapper(string projectName, string expected)
    {
        // Arrange
        var projectToAdd = new Project { Name = projectName };
        _dataMapperMock.Setup(s => s.AddProjectAsync(It.IsAny<Project>()))
                       .Returns(Task.CompletedTask);

        // Act
        await _sut.AddProjectAsync(projectToAdd);

        // Assert
        _dataMapperMock.Verify(s => s.AddProjectAsync(It.Is<Project>(p => p.Name == expected)), Times.Once);
    }

    [TestMethod]
    public async Task AddProjectAsync_ValidProjectName_ProjectService_CallsDataMapper()
    {
        // Arrange
        var projectToAdd = new Project { Name = "Smuggling-Pirates" };
        _dataMapperMock.Setup(s => s.AddProjectAsync(It.IsAny<Project>()))
                       .Returns(Task.CompletedTask);

        // Act
        await _sut.AddProjectAsync(projectToAdd);

        // Assert
        _dataMapperMock.Verify(s => s.AddProjectAsync(It.Is<Project>(p => p.Name == "Smuggling-Pirates")), Times.Once);
    }

    [TestMethod]
    public async Task DeleteProjectByNameAsync_ProjectService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _dataMapperMock.Setup(s => s.DeleteProjectByNameAsync(It.IsAny<string>()))
                       .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteProjectByNameAsync(projectName);

        // Assert
        _dataMapperMock.Verify(s => s.DeleteProjectByNameAsync("Smuggling-Pirates"), Times.Once);
    }
}
