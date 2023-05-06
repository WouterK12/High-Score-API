using HighScoreAPI.Models;
using HighScoreAPI.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HighScoreAPI.DAL.Mappings;

public class HighScoreMapping : IEntityTypeConfiguration<HighScore>
{
    public void Configure(EntityTypeBuilder<HighScore> builder)
    {
        builder.HasKey(hs => hs.Id);

        builder.Property(hs => hs.Username)
               .HasMaxLength(Constants.UsernameMaxLength)
               .IsRequired();

        builder.Property(hs => hs.Score)
               .IsRequired();

        builder.HasOne(hs => hs.Project)
               .WithMany(p => p.HighScores);
    }
}
