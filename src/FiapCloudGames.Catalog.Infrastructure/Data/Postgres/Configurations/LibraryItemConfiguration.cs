using FiapCloudGames.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Catalog.Infrastructure.Data.Relational.Configurations;

public class LibraryItemConfiguration : IEntityTypeConfiguration<LibraryItem>
{
    public void Configure(EntityTypeBuilder<LibraryItem> builder)
    {
        builder.ToTable("LibraryItems");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.UserId).IsRequired();
        builder.Property(l => l.AddedAt).IsRequired();

        builder.HasIndex(l => new { l.UserId, l.GameId })
            .IsUnique()
            .HasDatabaseName("UQ_LibraryItem_UserId_GameId");

        builder.HasOne(l => l.Game)
            .WithMany()
            .HasForeignKey(l => l.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Order)
            .WithMany()
            .HasForeignKey(l => l.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}