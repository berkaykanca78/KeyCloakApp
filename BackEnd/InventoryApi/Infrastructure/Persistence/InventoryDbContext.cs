using Microsoft.EntityFrameworkCore;
using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Infrastructure.Persistence;

/// <summary>
/// Inventory veritabanı (PostgreSQL) — Infrastructure katmanı, Domain aggregate'ini persist eder.
/// </summary>
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("InventoryItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Location).HasMaxLength(100).IsRequired();
            e.HasData(
                InventoryItem.CreateForSeed(1, "Ürün A", 100, "Depo-1"),
                InventoryItem.CreateForSeed(2, "Ürün B", 50, "Depo-1"),
                InventoryItem.CreateForSeed(3, "Ürün C", 200, "Depo-2"));
        });
    }
}
