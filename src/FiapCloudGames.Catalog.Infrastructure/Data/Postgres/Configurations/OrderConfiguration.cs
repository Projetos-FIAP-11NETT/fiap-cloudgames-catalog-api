using FiapCloudGames.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Catalog.Infrastructure.Data.Relational.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.UserId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.PaidAt);

        builder.HasOne(o => o.Game)
            .WithMany()
            .HasForeignKey(o => o.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}