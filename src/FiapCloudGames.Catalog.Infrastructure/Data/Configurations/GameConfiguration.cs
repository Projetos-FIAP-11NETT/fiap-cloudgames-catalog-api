using FiapCloudGames.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Catalog.Infrastructure.Data.Configurations;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(g => g.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Developer)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(g => g.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(g => g.ReleaseDate)
            .IsRequired();

        builder.HasIndex(g => new { g.Title, g.Developer })
            .IsUnique()
            .HasDatabaseName("UQ_Game_Title_Developer");

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