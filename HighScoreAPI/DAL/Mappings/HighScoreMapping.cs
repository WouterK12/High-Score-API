using HighScoreAPI.DAL;
using HighScoreServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HighScoreServer.DAL.Mappings;

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
