using HighScoreServer.DAL.Builders;
using HighScoreServer.DAL.Mappings;
using HighScoreServer.Models;
using Microsoft.EntityFrameworkCore;

namespace HighScoreServer.DAL;

public class HighScoreContext : DbContext
{
    public HighScoreContext() { }
    public HighScoreContext(DbContextOptions<HighScoreContext> options) : base(options) { }

    public DbSet<HighScore> HighScores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new HighScoreMapping());
    }
}
