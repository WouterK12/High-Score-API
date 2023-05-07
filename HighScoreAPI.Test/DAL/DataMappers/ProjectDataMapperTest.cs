using HighScoreAPI.DAL;
using HighScoreAPI.DAL.DataMappers;
using HighScoreAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.Test.DAL.DataMappers;

[TestClass]
public class ProjectDataMapperTest
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
    public async Task GetProjectByNameAsync_ProjectNotInDb_ProjectDataMapper_ReturnsNull()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";
        var sut = new ProjectDataMapper(_options);

        // Act
        var result = await sut.GetProjectByNameAsync(projectName);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetProjectByNameAsync_ProjectInDb_ProjectDataMapper_ReturnsProject()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";

        var highScore = new HighScore() { Username = "K03N", Score = 423 };
        var project = new Project() { Name = projectName, AesKeyBase64 = "key", HighScores = new HighScore[] { highScore } };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var sut = new ProjectDataMapper(_options);

        // Act
        var result = await sut.GetProjectByNameAsync(projectName);

        // Assert
        Assert.AreEqual("Smuggling-Pirates", result.Name);
        Assert.AreEqual(1, result.HighScores.Count);
        Assert.IsTrue(result.HighScores.Any(hs => hs.Username == "K03N" && hs.Score == 423));
    }

    [TestMethod]
    public async Task AddProjectAsync_ProjectNotInDb_ProjectDataMapper_AddsProjectToDb()
    {
        // Arrange
        var projectToAdd = new Project() { Name = "Smuggling-Pirates", AesKeyBase64 = "key" };
        var sut = new ProjectDataMapper(_options);

        // Act
        await sut.AddProjectAsync(projectToAdd);

        // Assert
        using var context = new DatabaseContext(_options);
        Assert.AreEqual(1, context.Projects.Count());
        Assert.IsTrue(context.Projects.Any(p => p.Name == "Smuggling-Pirates"));
    }

    [TestMethod]
    public async Task AddProjectAsync_ProjectInDb_ProjectDataMapper_DoesNotAddProjectToDb()
    {
        // Arrange
        string projectName = "Smuggling-Pirates";

        var project = new Project() { Name = projectName, AesKeyBase64 = "key" };
        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        var projectToAdd = new Project() { Name = projectName, AesKeyBase64 = "key" };
        var sut = new ProjectDataMapper(_options);

        // Act
        await sut.AddProjectAsync(projectToAdd);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        var projectsInDb = await assertContext.Projects.ToListAsync();
        Assert.AreEqual(1, projectsInDb.Count);
        Assert.IsTrue(projectsInDb.Any(p => p.Name == "Smuggling-Pirates"));
    }

    [TestMethod]
    public async Task DeleteProjectByNameAsync_ProjectDataMapper_DeletesProjectWithHighScores()
    {
        // Arrange
        var highScores = new List<HighScore>()
        {
            new() { Username = "K03N", Score = 423 },
            new() { Username = "Your Partner In Science", Score = 34 },
            new() { Username = "Vikko", Score = 83 },
            new() { Username = "UFOcreator4074", Score = 16 },
            new() { Username = "Hoeleboele", Score = 478 }
        };
        var project = new Project() { Name = "Smuggling-Pirates", AesKeyBase64 = "key", HighScores = highScores };

        using var context = new DatabaseContext(_options);
        context.Add(project);
        await context.SaveChangesAsync();

        string projectName = "Smuggling-Pirates";

        var sut = new ProjectDataMapper(_options);

        // Act
        await sut.DeleteProjectByNameAsync(projectName);

        // Assert
        using var assertContext = new DatabaseContext(_options);
        Assert.AreEqual(0, assertContext.Projects.Count());
        Assert.AreEqual(0, assertContext.HighScores.Count());
    }
}
