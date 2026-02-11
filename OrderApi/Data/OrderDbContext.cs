using Microsoft.EntityFrameworkCore;
using OrderApi.Entities;

namespace OrderApi.Data;

/// <summary>
/// Order veritabanı (MSSQL/T-SQL) için EF Core DbContext.
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
