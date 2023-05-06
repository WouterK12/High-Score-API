using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.DTOs;
using HighScoreAPI.Exceptions;
using HighScoreAPI.Models;
using HighScoreAPI.Services;
using Moq;
using ProfanityFilter.Interfaces;

namespace HighScoreAPI.Test.Services;

[TestClass]
public class HighScoreServiceTest
{
    private HighScoreService _sut = null!;

    private Mock<IHighScoreDataMapper> _highScoreDataMapperMock = null!;
    private Mock<IProjectDataMapper> _projectDataMapperMock = null!;
    private Mock<IProfanityFilter> _profanityFilterMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _highScoreDataMapperMock = new Mock<IHighScoreDataMapper>(MockBehavior.Strict);
        _projectDataMapperMock = new Mock<IProjectDataMapper>(MockBehavior.Strict);
        _profanityFilterMock = new Mock<IProfanityFilter>(MockBehavior.Strict);

        _sut = new HighScoreService(_highScoreDataMapperMock.Object, _projectDataMapperMock.Object, _profanityFilterMock.Object);
    }

    [TestMethod]
    public async Task GetTopAsync_HighScoreService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        int amount = 10;
        var highScores = new List<HighScoreDTO>()
        {
            new("K03N", 423),
            new("Your Partner In Science", 34)
        };
        _highScoreDataMapperMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(highScores);

        // Act
        var result = await _sut.GetTopAsync(projectName, amount);

        // Assert
        _highScoreDataMapperMock.Verify(dm => dm.GetTopAsync("Smuggling-Pirates", 10), Times.Once);
        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.Any(hs => hs.Username == "K03N" && hs.Score == 423));
        Assert.IsTrue(result.Any(hs => hs.Username == "Your Partner In Science" && hs.Score == 34));
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    public async Task GetTopAsync_LessOrEqual0_HighScoreService_ThrowsArgumentOutOfRangeException(int amount)
    {
        // Arrange
        string projectName = "Smuggling-Pirates";

        // Act
        async Task<IEnumerable<HighScoreDTO>> Act()
        {
            return await _sut.GetTopAsync(projectName, amount);
        }

        // Assert
        ArgumentOutOfRangeException ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(Act);
        Assert.AreEqual("The amount must be greater than 0!", ex.Message);
    }

    [TestMethod]
    public async Task GetTopAsync_1_HighScoreService_DoesNotThrowArgumentOutOfRangeException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        int amount = 1;
        _highScoreDataMapperMock.Setup(s => s.GetTopAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(It.IsAny<IEnumerable<HighScoreDTO>>());

        try
        {
            // Act
            await _sut.GetTopAsync(projectName, amount);
        }
        catch (Exception)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var highScore = new HighScoreDTO(username, 423);
        _highScoreDataMapperMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(highScore);

        // Act
        var result = await _sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        _highScoreDataMapperMock.Verify(dm => dm.GetHighScoreByUsernameAsync("Smuggling-Pirates", "K03N"), Times.Once);
        Assert.AreEqual("K03N", result.Username);
        Assert.AreEqual(423, result.Score);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreService_DataMapperReturnsNull_ThrowsHighScoreNotFoundException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        _highScoreDataMapperMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => null!);

        // Act
        async Task<HighScoreDTO> Act()
        {
            return await _sut.GetHighScoreByUsernameAsync(projectName, username);
        }

        // Assert
        HighScoreNotFoundException ex = await Assert.ThrowsExceptionAsync<HighScoreNotFoundException>(Act);
        Assert.AreEqual("High Score for user with username \"K03N\" could not be found.", ex.Message);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task AddHighScoreAsync_Score0OrLess_HighScoreService_ThrowsInvalidHighScoreException(long score)
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var highScoreToAdd = new HighScoreDTO(username, score);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(username);

        // Act
        async Task Act()
        {
            await _sut.AddHighScoreAsync(projectName, highScoreToAdd);
        }

        // Assert
        InvalidHighScoreException ex = await Assert.ThrowsExceptionAsync<InvalidHighScoreException>(Act);
        Assert.AreEqual("Your Score must be greater than 0!", ex.Message);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_Score1_HighScoreService_DoesNotThrowException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var project = new Project() { Name = projectName };
        var highScoreToAdd = new HighScoreDTO(username, 1);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(username);

        try
        {
            // Act
            await _sut.AddHighScoreAsync(projectName, highScoreToAdd);
        }
        catch (Exception)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    [DataRow("WowAnExtremelyMegaSuperLongUsernameWithTotalLengthOf65Characters!")]
    public async Task AddHighScoreAsync_UsernameNullOrWhiteSpaceOrLongerThan64_HighScoreService_ThrowsInvalidHighScoreException(string username)
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO(username, 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        async Task Act()
        {
            await _sut.AddHighScoreAsync(projectName, highScoreToAdd);
        }

        // Assert
        InvalidHighScoreException ex = await Assert.ThrowsExceptionAsync<InvalidHighScoreException>(Act);
        Assert.AreEqual("Your Username must be between 1 and 64 characters!", ex.Message);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_UsernameLength64_HighScoreService_DoesNotThrowException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var usernameWithLength64 = "OhAnExtremelyMegaSuperLongUsernameWithTotalLengthOf64Characters!";
        var project = new Project() { Name = projectName };
        var highScoreToAdd = new HighScoreDTO(usernameWithLength64, 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(usernameWithLength64);

        try
        {
            // Act
            await _sut.AddHighScoreAsync(projectName, highScoreToAdd);
        }
        catch (Exception)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
    }

    [TestMethod]
    public async Task AddHighScoreAsync_CallsProfanityFilter()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var project = new Project() { Name = projectName };
        var highScoreToAdd = new HighScoreDTO(username, 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(username);

        // Act
        await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        _profanityFilterMock.Verify(pf => pf.CensorString(username, ' '), Times.Once);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public async Task AddHighScoreAsync_ProfanityFilterReturnsWhiteSpaces_HighScoreService_ThrowsInvalidHighScoreException(string profanityFilterResult)
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(profanityFilterResult);

        // Act
        async Task Act()
        {
            await _sut.AddHighScoreAsync(projectName, highScoreToAdd);
        }

        // Assert
        InvalidHighScoreException ex = await Assert.ThrowsExceptionAsync<InvalidHighScoreException>(Act);
        Assert.AreEqual("Your Username must be between 1 and 64 characters!", ex.Message);
    }

    [DataTestMethod]
    [DataRow(" K03N", "K03N")]
    [DataRow("  Desox", "Desox")]
    [DataRow("Your   Partner In  Science", "Your Partner In Science")]
    [DataRow("Vikko ", "Vikko")]
    [DataRow("UFOcreator4074  ", "UFOcreator4074")]
    [DataRow(" Hoeleboele ", "Hoeleboele")]
    [DataRow("  Jarno2212  ", "Jarno2212")]
    public async Task AddHighScoreAsync_ProfanityFilterReturnsUsernameWithWhiteSpaces_HighScoreService_CallsDataMapper(string filteredUsername, string expected)
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var project = new Project() { Name = projectName };
        var highScoreToAdd = new HighScoreDTO(filteredUsername, 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(filteredUsername);

        // Act
        await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        _highScoreDataMapperMock.Verify(s => s.AddHighScoreAsync(
            "Smuggling-Pirates",
            It.Is<HighScoreDTO>(hs =>
                hs.Username == expected &&
                hs.Score == 423)
            ), Times.Once);
    }

    public async Task AddHighScoreAsync_ProjectDataMapperReturnsNull_HighScoreService_ThrowsProjectNotFoundException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null!);

        // Act
        async Task Act()
        {
            await _sut.AddHighScoreAsync(projectName, highScoreToAdd);
        }

        // Assert
        ProjectNotFoundException ex = await Assert.ThrowsExceptionAsync<ProjectNotFoundException>(Act);
        Assert.AreEqual("Project with name \"Smuggling-Pirates\" could not be found.", ex.Message);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_ValidHighScore_HighScoreService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var project = new Project() { Name = projectName };
        var highScoreToAdd = new HighScoreDTO(username, 423);
        _highScoreDataMapperMock.Setup(s => s.AddHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);
        _projectDataMapperMock.Setup(s => s.GetProjectByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(project);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
            .Returns(username);

        // Act
        await _sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        _highScoreDataMapperMock.Verify(s => s.AddHighScoreAsync(
            "Smuggling-Pirates",
            It.Is<HighScoreDTO>(hs =>
                hs.Username == "K03N" &&
                hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreService_CallsDataMapper()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        _highScoreDataMapperMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(highScoreToDelete);
        _highScoreDataMapperMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteHighScoreAsync(projectName, highScoreToDelete);

        // Assert
        _highScoreDataMapperMock.Verify(s => s.DeleteHighScoreAsync(
            "Smuggling-Pirates",
            It.Is<HighScoreDTO>(hs =>
                hs.Username == "K03N" &&
                hs.Score == 423)),
            Times.Once);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreService_DataMapperReturnsNull_ThrowsHighScoreNotFoundException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        _highScoreDataMapperMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => null!);
        _highScoreDataMapperMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);

        // Act

        async Task Act()
        {
            await _sut.DeleteHighScoreAsync(projectName, highScoreToDelete);
        }

        // Assert
        HighScoreNotFoundException ex = await Assert.ThrowsExceptionAsync<HighScoreNotFoundException>(Act);
        Assert.AreEqual("High Score for user with username \"K03N\" and score \"423\" could not be found.", ex.Message);
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreService_DataMapperReturnsDifferentScore_ThrowsHighScoreNotFoundException()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        _highScoreDataMapperMock.Setup(s => s.GetHighScoreByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new HighScoreDTO("K03N", 422));
        _highScoreDataMapperMock.Setup(s => s.DeleteHighScoreAsync(It.IsAny<string>(), It.IsAny<HighScoreDTO>()))
            .Returns(Task.CompletedTask);

        // Act

        async Task Act()
        {
            await _sut.DeleteHighScoreAsync(projectName, highScoreToDelete);
        }

        // Assert
        HighScoreNotFoundException ex = await Assert.ThrowsExceptionAsync<HighScoreNotFoundException>(Act);
        Assert.AreEqual("High Score for user with username \"K03N\" and score \"423\" could not be found.", ex.Message);
    }
}
