using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.DTOs;
using HighScoreAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.Test.DAL.DataMappers;

[TestClass]
public class HighScoreDataMapperTest
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<DatabaseContext> _options = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(_connection)
            .Options;

        using DatabaseContext context = new(_options);
        context.Database.EnsureCreated();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _connection.Dispose();
    }

    [TestMethod]
    public async Task GetTopAsync_ProjectNotInDb_HighScoreDataMapper_ReturnsNoElements()
    {
        // Arrange
        var highScores = new List<HighScore>()
        {
            new() { Username = "Yvi", Score = 9241 },
            new() { Username = "Physician", Score = 10321 },
        };
        var project = new Project() { Name = "Pointless-Harvest", HighScores = highScores };

        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        string projectName = "Smuggling-Pirates";
        int amount = 10;

        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetTopAsync(projectName, amount);

        // Assert
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task GetTopAsync_10_HighScoreDataMapper_ReturnsTop10ScoreDescending()
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
        var otherHighScores = new List<HighScore>()
        {
            new() { Username = "Yvi", Score = 9241 },
            new() { Username = "Physician", Score = 10321 },
        };
        var project1 = new Project() { Name = "Smuggling-Pirates", HighScores = all12HighScores };
        var project2 = new Project() { Name = "Pointless-Harvest", HighScores = otherHighScores };

        using var context = new DatabaseContext(_options);
        context.Add(project1);
        context.Add(project2);
        await context.SaveChangesAsync();

        string projectName = "Smuggling-Pirates";
        int amount = 10;

        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetTopAsync(projectName, amount);

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
    public async Task GetHighScoreByUsernameAsync_HighScoreNotInDb_HighScoreDataMapper_ReturnsNull()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";
        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetHighScoreByUsernameAsync_HighScoreInDb_HighScoreDataMapper_ReturnsHighScore()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";

        var highScore = new HighScore() { Username = username, Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var sut = new HighScoreDataMapper(_options);

        // Act
        var result = await sut.GetHighScoreByUsernameAsync(projectName, username);

        // Assert
        Assert.AreEqual("K03N", result.Username);
        Assert.AreEqual(423, result.Score);
    }

    [TestMethod]
    public async Task AddHighScoreAsync_ProjectNotInDb_HighScoreDataMapper_AddsProjectToDb()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var project = new Project() { Name = projectName };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        Assert.AreEqual(1, assertContext.Projects.Count());
        Assert.IsTrue(assertContext.Projects.Any(p => p.Name == "Smuggling-Pirates"));
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScoreNotInDb_HighScoreDataMapper_AddsHighScoreToDb()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var project = new Project() { Name = projectName };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var highScoreToAdd = new HighScoreDTO("K03N", 423);
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        var projectInDb = await assertContext.Projects.Include(p => p.HighScores).SingleAsync();
        Assert.AreEqual("Smuggling-Pirates", projectInDb.Name);
        Assert.AreEqual(1, projectInDb.HighScores.Count());
        Assert.IsTrue(projectInDb.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScoreHigherThanInDb_HighScoreDataMapper_UpdatesHighScoreInDb()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";

        var highScore = new HighScore() { Username = username, Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var highScoreToAdd = new HighScoreDTO("K03N", 8893);
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        var projectInDb = await assertContext.Projects.Include(p => p.HighScores).SingleAsync();
        Assert.AreEqual("Smuggling-Pirates", project.Name);
        Assert.AreEqual(1, projectInDb.HighScores.Count());
        Assert.IsTrue(projectInDb.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 8893));
    }

    [TestMethod]
    public async Task AddHighScoreAsync_HighScoreLowerThanInDb_HighScoreDataMapper_DoesNotUpdateHighScoreInDb()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        string username = "K03N";

        var highScore = new HighScore() { Username = username, Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var highScoreToAdd = new HighScoreDTO("K03N", 422);
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.AddHighScoreAsync(projectName, highScoreToAdd);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        var projectInDb = await assertContext.Projects.Include(p => p.HighScores).SingleAsync();
        Assert.AreEqual("Smuggling-Pirates", project.Name);
        Assert.AreEqual(1, projectInDb.HighScores.Count());
        Assert.IsTrue(projectInDb.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreDataMapper_DeletesHighScore()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var highScoreToDelete = new HighScoreDTO("K03N", 423);
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.DeleteHighScoreAsync(projectName, highScoreToDelete);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        Assert.AreEqual(0, assertContext.HighScores.Count());
    }

    [TestMethod]
    public async Task DeleteHighScoreAsync_HighScoreNotInDb_HighScoreDataMapper_DoesNotDeleteHighScore()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        var project = new Project() { Name = projectName, HighScores = new HighScore[] { highScore } };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var highScoreToDelete = new HighScoreDTO("K03N", 422);
        var sut = new HighScoreDataMapper(_options);

        // Act
        await sut.DeleteHighScoreAsync(projectName, highScoreToDelete);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        Assert.AreEqual(1, assertContext.HighScores.Count());
        Assert.IsTrue(assertContext.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }
}
