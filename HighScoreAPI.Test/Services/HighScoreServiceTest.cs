using HighScoreAPI.DAL.DataMappers;
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

    private Mock<IHighScoreDataMapper> _dataMapperMock = null!;
    private Mock<IProfanityFilter> _profanityFilterMock = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _dataMapperMock = new Mock<IHighScoreDataMapper>(MockBehavior.Strict);
        _profanityFilterMock = new Mock<IProfanityFilter>(MockBehavior.Strict);

        _sut = new HighScoreService(_dataMapperMock.Object, _profanityFilterMock.Object);
    }

    [TestMethod]
    public async Task GetTop_10_HighScoreService_CallsDataMapper()
    {
        // Arrange
        int amount = 10;
        var highScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 }
        };
        _dataMapperMock.Setup(s => s.GetTop(It.IsAny<int>()))
                       .ReturnsAsync(highScores);

        // Act
        var result = await _sut.GetTop(amount);

        // Assert
        _dataMapperMock.Verify(dm => dm.GetTop(10), Times.Once);
        Assert.AreEqual(2, result.Count());
        Assert.IsTrue(result.Any(hs => hs.Username == "K03N" && hs.Score == 423));
        Assert.IsTrue(result.Any(hs => hs.Username == "Your Partner In Science" && hs.Score == 34));
    }

    [DataTestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    public async Task GetTop_LessOrEqual0_HighScoreService_ThrowsArgumentOutOfRangeException(int amount)
    {
        // Act
        async Task<IEnumerable<HighScore>> Act()
        {
            return await _sut.GetTop(amount);
        }

        // Assert
        ArgumentOutOfRangeException ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(Act);
        Assert.AreEqual("The amount must be greater than 0!", ex.Message);
    }

    [TestMethod]
    public async Task GetTop_1_HighScoreService_DoesNotThrowArgumentOutOfRangeException()
    {
        // Arrange
        int amount = 1;
        _dataMapperMock.Setup(s => s.GetTop(It.IsAny<int>()))
                       .ReturnsAsync(It.IsAny<IEnumerable<HighScore>>());

        try
        {
            // Act
            await _sut.GetTop(amount);
        }
        catch (Exception)
        {
            // Assert
            // Should not throw Exception
            Assert.Fail();
        }
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
    [DataRow(0)]
    [DataRow(-1)]
    public async Task AddHighScore_Score0OrLess_HighScoreService_ThrowsInvalidHighScoreException(long score)
    {
        // Arrange
        var username = "K03N";
        var highScoreToAdd = new HighScore { Username = username, Score = score };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(username);

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
        var username = "K03N";
        var highScoreToAdd = new HighScore { Username = username, Score = 1 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(username);

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
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
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
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(usernameWithLength30);

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

    [TestMethod]
    public async Task AddHighScore_CallsProfanityFilter()
    {
        // Arrange
        var username = "K03N";
        var highScoreToAdd = new HighScore { Username = username, Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(username);

        // Act
        await _sut.AddHighScore(highScoreToAdd);

        // Assert
        _profanityFilterMock.Verify(pf => pf.CensorString(username, ' '), Times.Once);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("  ")]
    public async Task AddHighScore_ProfanityFilterReturnsWhiteSpaces_HighScoreService_ThrowsInvalidHighScoreException(string profanityFilterResult)
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = "K03N", Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(profanityFilterResult);

        // Act
        async Task Act()
        {
            await _sut.AddHighScore(highScoreToAdd);
        }

        // Assert
        InvalidHighScoreException ex = await Assert.ThrowsExceptionAsync<InvalidHighScoreException>(Act);
        Assert.AreEqual("Your Username must be between 1 and 30 characters!", ex.Message);
    }

    [DataTestMethod]
    [DataRow(" K03N", "K03N")]
    [DataRow("  Desox", "Desox")]
    [DataRow("Your   Partner In  Science", "Your Partner In Science")]
    [DataRow("Vikko ", "Vikko")]
    [DataRow("UFOcreator4074  ", "UFOcreator4074")]
    [DataRow(" Hoeleboele ", "Hoeleboele")]
    [DataRow("  Jarno2212  ", "Jarno2212")]
    public async Task AddHighScore_ProfanityFilterReturnsUsernameWithWhiteSpaces_HighScoreService_CallsDataMapper(string filteredUsername, string expected)
    {
        // Arrange
        var highScoreToAdd = new HighScore { Username = filteredUsername, Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(filteredUsername);

        // Act
        await _sut.AddHighScore(highScoreToAdd);

        // Assert
        _dataMapperMock.Verify(s => s.AddHighScore(It.Is<HighScore>(hs =>
            hs.Username == expected &&
            hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task AddHighScore_ValidHighScore_HighScoreService_CallsDataMapper()
    {
        // Arrange
        var username = "K03N";
        var highScoreToAdd = new HighScore { Username = username, Score = 423 };
        _dataMapperMock.Setup(s => s.AddHighScore(It.IsAny<HighScore>()))
                       .Returns(Task.CompletedTask);
        _profanityFilterMock.Setup(pf => pf.CensorString(It.IsAny<string>(), It.IsAny<char>()))
                            .Returns(username);

        // Act
        await _sut.AddHighScore(highScoreToAdd);

        // Assert
        _dataMapperMock.Verify(s => s.AddHighScore(It.Is<HighScore>(hs =>
            hs.Username == "K03N" &&
            hs.Score == 423)
            ), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAllHighScores_HighScoreService_CallsDataMapper()
    {
        // Arrange
        _dataMapperMock.Setup(s => s.DeleteAllHighScores())
                       .Returns(Task.CompletedTask);

        // Act
        await _sut.DeleteAllHighScores();

        // Assert
        _dataMapperMock.Verify(s => s.DeleteAllHighScores(), Times.Once);
    }
}
