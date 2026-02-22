using FiapCloudGames.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Catalog.Infrastructure.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasMany(g => g.Categories)
            .WithMany(c => c.Games)
            .UsingEntity<Dictionary<string, object>>(
                "GameCategory",
                j => j.HasOne<Category>().WithMany().HasForeignKey("CategoryId"),
                j => j.HasOne<Game>().WithMany().HasForeignKey("GameId"),
                j =>
                {
                    j.HasKey("GameId", "CategoryId");
                    j.ToTable("GameCategory");
                });
    }
}