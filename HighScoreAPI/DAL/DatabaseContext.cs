using HighScoreAPI.DAL.Mappings;
using HighScoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.DAL;

public class DatabaseContext : DbContext
{
    public DatabaseContext() { }
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<HighScore> HighScores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProjectMapping());
        modelBuilder.ApplyConfiguration(new HighScoreMapping());
    }
}
