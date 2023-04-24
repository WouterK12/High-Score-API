using HighScoreAPI.DAL;
using HighScoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HighScoreAPI.DAL.Mappings;

public class HighScoreMapping : IEntityTypeConfiguration<HighScore>
{
    public void Configure(EntityTypeBuilder<HighScore> builder)
    {
        builder.HasKey(hs => hs.Username);

        builder.Property(hs => hs.Username)
               .HasMaxLength(HighScoreProperties.UsernameMaxLength)
               .IsRequired();

        builder.Property(hs => hs.Score)
               .IsRequired();
    }
}
