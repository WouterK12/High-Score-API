using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.Test.DAL.DataMappers;

[TestClass]
public class HighScoreDataMapperTest
{
    private SqliteConnection _connection;
    private DbContextOptions<HighScoreContext> _options;

    [TestInitialize]
    public void TestInitialize()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<HighScoreContext>()
            .UseSqlite(_connection)
            .Options;

        using HighScoreContext context = new HighScoreContext(_options);
        context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _connection.Dispose();
    }

    [TestMethod]
    public async Task GetTop10_HighScoreDataMapper_ReturnsTop10ScoreDescending()
    {
        // Arrange
        var all12HighScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 },
            new() { Username = "Vikko", Score = 83 },
            new() { Username = "UFOcreator4074", Score = 16 },
            new() { Username = "Hoeleboele", Score = 478 },
            new() { Username = "Jarno2212", Score = 183 },
            new() { Username = "Desox", Score = 772 },
            new() { Username = "Guit", Score = 4 },
            new() { Username = "Hasji", Score = 23 },
            new() { Username = "Exotic", Score = 991 },
            new() { Username = "MRjasperDR", Score = 8274 },
            new() { Username = "Opblaas Doggo", Score = 603 },
        };
        using var context = new HighScoreContext(_options);
        context.AddRange(all12HighScores);
        context.SaveChanges();

        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetTop10();

        // Assert
        var resultList = result.ToList();
        Assert.AreEqual(10, resultList.Count);
        Assert.IsTrue(resultList[0].Username == "MRjasperDR" && resultList[0].Score == 8274);
        Assert.IsTrue(resultList[1].Username == "Exotic" && resultList[1].Score == 991);
        Assert.IsTrue(resultList[2].Username == "Desox" && resultList[2].Score == 772);
        Assert.IsTrue(resultList[3].Username == "Opblaas Doggo" && resultList[3].Score == 603);
        Assert.IsTrue(resultList[4].Username == "Hoeleboele" && resultList[4].Score == 478);
        Assert.IsTrue(resultList[5].Username == "K03N" && resultList[5].Score == 423);
        Assert.IsTrue(resultList[6].Username == "Jarno2212" && resultList[6].Score == 183);
        Assert.IsTrue(resultList[7].Username == "Vikko" && resultList[7].Score == 83);
        Assert.IsTrue(resultList[8].Username == "Your Partner In Science" && resultList[8].Score == 34);
        Assert.IsTrue(resultList[9].Username == "Hasji" && resultList[9].Score == 23);
    }

    [TestMethod]
    public async Task GetHighScoreByUsername_UsernameNotInDb_HighScoreDataMapper_ReturnsNull()
    {
        // Arrange
        var username = "K03N";
        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetHighScoreByUsername(username);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetHighScoreByUsername_UsernameInDb_HighScoreDataMapper_ReturnsHighScore()
    {
        // Arrange
        var username = "K03N";

        var highScore = new HighScore() { Username = username, Score = 423 };
        using var context = new HighScoreContext(_options);
        context.Add(highScore);
        context.SaveChanges();

        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetHighScoreByUsername(username);

        // Assert
        Assert.AreEqual("K03N", result.Username);
        Assert.AreEqual(423, result.Score);
    }

    [TestMethod]
    public async Task AddHighScore_HighScoreNotInDb_HighScoreDataMapper_AddsToDb()
    {
        // Arrange
        var highScoreToAdd = new HighScore() { Username = "K03N", Score = 423 };
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScore(highScoreToAdd);

        // Assert
        using var context = new HighScoreContext(_options);
        Assert.AreEqual(1, context.HighScores.Count());
        Assert.IsTrue(context.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }

    [TestMethod]
    public async Task AddHighScore_HighScoreHigherThanInDb_HighScoreDataMapper_UpdatesHighScoreInDb()
    {
        // Arrange
        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        using var context = new HighScoreContext(_options);
        context.Add(highScore);
        context.SaveChanges();

        var highScoreToAdd = new HighScore() { Username = "K03N", Score = 8893 };
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScore(highScoreToAdd);

        // Assert
        using var assertContext = new HighScoreContext(_options);
        Assert.AreEqual(1, assertContext.HighScores.Count());
        Assert.IsTrue(assertContext.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 8893));
    }

    [TestMethod]
    public async Task AddHighScore_HighScoreLowerThanInDb_HighScoreDataMapper_DoesNotUpdateHighScoreInDb()
    {
        // Arrange
        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        using var context = new HighScoreContext(_options);
        context.Add(highScore);
        context.SaveChanges();

        var highScoreToAdd = new HighScore() { Username = "K03N", Score = 422 };
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScore(highScoreToAdd);

        // Assert
        using var assertContext = new HighScoreContext(_options);
        Assert.AreEqual(1, assertContext.HighScores.Count());
        Assert.IsTrue(assertContext.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }
}
