using Microsoft.EntityFrameworkCore;
using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Infrastructure.Persistence;

/// <summary>
/// Inventory veritabanı (PostgreSQL) — Product, Warehouse, InventoryItem. Tüm PK'ler Guid; seed yok.
/// </summary>
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.ImageKey).HasMaxLength(500);
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.ToTable("Warehouses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.ImageKey).HasMaxLength(500);
        });

        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("InventoryItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.ImageKey).HasMaxLength(500);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
