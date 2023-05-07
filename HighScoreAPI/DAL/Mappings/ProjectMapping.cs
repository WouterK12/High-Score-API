using HighScoreAPI.Models;
using HighScoreAPI.Properties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HighScoreAPI.DAL.Mappings;

public class ProjectMapping : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Name);

        builder.Property(p => p.Name)
               .HasMaxLength(Constants.ProjectNameMaxLength)
               .IsRequired();

        builder.Property(p => p.AesKeyBase64)
               .HasMaxLength(Constants.MaxEncryptionKeyLength)
               .IsRequired();

        builder.HasMany(p => p.HighScores)
               .WithOne(hs => hs.Project);
    }
}
