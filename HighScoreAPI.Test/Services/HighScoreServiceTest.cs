using HighScoreServer.DAL.DataMappers;
using HighScoreServer.Exceptions;
using HighScoreServer.Models;
using HighScoreServer.Services;
using Moq;

namespace HighScoreServer.Test.Services;

[TestClass]
public class HighScoreServiceTest
{
    private HighScoreService _sut = null!;

    private Mock<IHighScoreDataMapper> _dataMapperMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _dataMapperMock = new Mock<IHighScoreDataMapper>(MockBehavior.Strict);

        _sut = new HighScoreService(_dataMapperMock.Object);
    }

    [TestMethod]
    public async Task GetTop10_HighScoreService_CallsDataMapper()
    {
        // Arrange
        var highScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 }
        };
        _dataMapperMock.Setup(s => s.GetTop10())
                       .ReturnsAsync(highScores);

        // Act
        var result = await _sut.GetTop10();

        // Assert
        _dataMapperMock.Verify(dm => dm.GetTop10(), Times.Once);
        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.Any(hs => hs.Username == "K03N" && hs.Score == 423));
        Assert.IsTrue(result.Any(hs => hs.Username == "Your Partner In Science" && hs.Score == 34));
    }

    [TestMethod]
    public async Task GetHighScoreByUsername_HighScoreService_CallsDataMapper()
    {
        // Arrange
        var username = "K03N";
        var highScore = new HighScore() { Username = username, Score = 423 };
        _dataMapperMock.Setup(s => s.GetHighScoreByUsername(It.IsAny<string>()))
                       .ReturnsAsync(highScore);

        // Act
        var result = await _sut.GetHighScoreByUsername(username);

        // Assert
        _dataMapperMock.Verify(dm => dm.GetHighScoreByUsername("K03N"), Times.Once);
        Assert.AreEqual("K03N", result.Username);
        Assert.AreEqual(423, result.Score);
    }

    [TestMethod]
    public async Task GetHighScoreByUsername_HighScoreService_DataMapperReturnsNull_ThrowsHighScoreNotFoundException()
    {
        // Arrange
        var username = "K03N";
        _dataMapperMock.Setup(s => s.GetHighScoreByUsername(It.IsAny<string>()))
                       .ReturnsAsync(() => null!);

        // Act
        async Task<HighScore> Act()
        {
            return await _sut.GetHighScoreByUsername(username);
        }

        // Assert
        HighScoreNotFoundException ex = await Assert.ThrowsExceptionAsync<HighScoreNotFoundException>(Act);
        Assert.AreEqual("High Score for user with username \"K03N\" could not be found.", ex.Message);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("LongUsernameWithTotalLength31!!")]
    public async Task AddHighScore_UsernameNullOrWhiteSpaceOrLongerThan30_HighScoreService_ThrowsInvalidHighScoreException(string username)
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = username, Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);

        // Act
        async Task Act()
        {
            await _sut.AddHighScore(highScoreToAdd);
        }

        // Assert
        InvalidHighScoreException ex = await Assert.ThrowsExceptionAsync<InvalidHighScoreException>(Act);
        Assert.AreEqual("Your Username must be between 1 and 30 characters!", ex.Message);
    }

    [TestMethod]
    public async Task AddHighScore_UsernameLength30_HighScoreService_DoesNotThrowException()
    {
        // Arrange
        var usernameWithLength30 = "LongUsernameWithTotalLength30!";
        var highScoreToAdd = new HighScore { Username = usernameWithLength30, Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);

        try
        {
            // Act
            await _sut.AddHighScore(highScoreToAdd);
        }
        catch (Exception)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task AddHighScore_Score0OrLess_HighScoreService_ThrowsInvalidHighScoreException(long score)
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = score };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);

        // Act
        async Task Act()
        {
            await _sut.AddHighScore(highScoreToAdd);
        }

        // Assert
        InvalidHighScoreException ex = await Assert.ThrowsExceptionAsync<InvalidHighScoreException>(Act);
        Assert.AreEqual("Your Score must be greater than 0!", ex.Message);
    }

    [TestMethod]
    public async Task AddHighScore_Score1_HighScoreService_DoesNotThrowException()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 1 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);

        try
        {
            // Act
            await _sut.AddHighScore(highScoreToAdd);
        }
        catch (Exception ex)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
    }

    [TestMethod]
    public async Task AddHighScore_ValidHighScore_HighScoreService_CallsDataMapper()
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);

        // Act
        await _sut.AddHighScore(highScoreToAdd);

        // Assert
        _dataMapperMock.Verify(s => s.AddHighScore(It.Is<HighScore>(hs =>
            hs.Username == "K03N" &&
            hs.Score == 423)
            ), Times.Once);
    }
}
