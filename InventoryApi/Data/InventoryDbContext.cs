using Microsoft.EntityFrameworkCore;
using InventoryApi.Entities;

namespace InventoryApi.Data;

/// <summary>
/// Inventory veritabanı (PostgreSQL) için EF Core DbContext.
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
                new InventoryItem { Id = 1, ProductName = "Ürün A", Quantity = 100, Location = "Depo-1" },
                new InventoryItem { Id = 2, ProductName = "Ürün B", Quantity = 50, Location = "Depo-1" },
                new InventoryItem { Id = 3, ProductName = "Ürün C", Quantity = 200, Location = "Depo-2" });
        });
    }
}
