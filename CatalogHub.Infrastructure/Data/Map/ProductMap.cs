using CatalogHub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogHub.Infrastructure.Data.Map;

public class ProductMap : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .HasColumnName("id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(p => p.Name)
               .HasColumnName("name")
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Description)
               .HasColumnName("description")
               .HasMaxLength(2000)
               .IsRequired();

        builder.Property(p => p.Price)
               .HasColumnName("price")
               .HasColumnType("real")
               .IsRequired();

        builder.Property(p => p.StockQuantity)
                .HasColumnName("stock_quantity")
                .IsRequired();

        builder.Property(p => p.IsActive)
               .HasColumnName("is_active")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(p => p.ImageUrl)
               .HasColumnName("image_url")
               .HasMaxLength(1000);

        builder.Property(p => p.CategoryId)
               .HasColumnName("category_id")
               .HasColumnType("uuid")
               .IsRequired();

        builder.Property(p => p.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamp with time zone")
               .HasDefaultValueSql("now()")
               .IsRequired();

        builder.Property(p => p.UpdatedAt)
               .HasColumnName("updated_at")
               .HasColumnType("timestamp with time zone");

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
