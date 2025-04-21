using Microsoft.EntityFrameworkCore;
using ShoppingModular.Domain.Orders;

namespace ShoppingModular.Infrastructure.Orders;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
        });
    }
}