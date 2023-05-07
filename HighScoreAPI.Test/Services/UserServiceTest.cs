using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Services;
using Moq;

namespace HighScoreAPI.Test.Services;

[TestClass]
public class UserServiceTest
{
    private UserService _sut = null!;

    private Mock<IProjectDataMapper> _dataMapperMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _dataMapperMock = new Mock<IProjectDataMapper>(MockBehavior.Strict);

        _sut = new UserService(_dataMapperMock.Object);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var project = new Project() { Name = projectName };
        _dataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);

        // Act
        await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        _dataMapperMock.Verify(dm => dm.GetProjectByNameAsync("Smuggling-Pirates"), Times.Once);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserService_DataMapperReturnsNull_ThrowsProjectNotFoundException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        _dataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null!);

        // Act
        async Task<string> Act()
        {
            return await _sut.GetRandomUsernameAsync(projectName);
        }

        // Assert
        ProjectNotFoundException ex = await Assert.ThrowsExceptionAsync<ProjectNotFoundException>(Act);
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", ex.Message);
    }

    [TestMethod]
    public async Task GetRandomUsernameAsync_UserService_DoesNotReturnNameReturnedByDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var project = new Project()
        {
            Name = projectName,
            HighScores = new HighScore[]
            {
                new() { Username = "K03N" },
                new() { Username = "Your Partner In Science" }
            }
        };
        _dataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);

        // Act
        var result = await _sut.GetRandomUsernameAsync(projectName);

        // Assert
        _dataMapperMock.Verify(dm => dm.GetProjectByNameAsync("Smuggling-Pirates"), Times.Once);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result));
        Assert.AreNotEqual("K03N", result);
        Assert.AreNotEqual("Your Partner In Science", result);
    }
}
