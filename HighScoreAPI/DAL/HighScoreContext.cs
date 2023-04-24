using HighScoreAPI.DAL.Builders;
using HighScoreAPI.DAL.Mappings;
using HighScoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HighScoreAPI.DAL;

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
