using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Aggregates;

namespace OrderApi.Infrastructure.Persistence;

/// <summary>
/// Order veritabanı (MSSQL) — Infrastructure katmanı, Domain aggregate'ini persist eder.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
            e.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        });
    }
}
