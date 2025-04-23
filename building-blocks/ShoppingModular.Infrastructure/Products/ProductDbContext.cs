using Microsoft.EntityFrameworkCore;
using ShoppingModular.Domain.Products;

namespace ShoppingModular.Infrastructure.Products;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Stock)
                .IsRequired();

            builder.Property(x => x.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Rating)
                .HasColumnType("decimal(3,2)");

            builder.Property(x => x.ReviewCount)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            // Conversão simples para listas (Tags e Images)
            builder.Property(x => x.Tags)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            builder.Property(x => x.Images)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
    }
}