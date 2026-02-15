using Microsoft.EntityFrameworkCore;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Infrastructure.Persistence;

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
    public DbSet<ProductDiscount> ProductDiscounts => Set<ProductDiscount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name)
                .HasConversion(v => v.Value, v => new ProductName(v))
                .HasMaxLength(200).IsRequired();
            e.Property(x => x.ImageKey).HasMaxLength(500);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<ProductDiscount>(e =>
        {
            e.ToTable("ProductDiscounts");
            e.HasKey(x => x.Id);
            e.Property(x => x.DiscountPercent)
                .HasConversion(v => v.Value, v => new DiscountPercent(v))
                .HasPrecision(5, 2);
            e.Property(x => x.Name).HasMaxLength(200);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.ProductId, x.StartAt, x.EndAt });
        });

        modelBuilder.Entity<Warehouse>(e =>
        {
            e.ToTable("Warehouses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name)
                .HasConversion(v => v.Value, v => new Location(v))
                .HasMaxLength(100).IsRequired();
            e.Property(x => x.Code).HasMaxLength(20);
            e.Property(x => x.ImageKey).HasMaxLength(500);
        });

        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("InventoryItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.Quantity)
                .HasConversion(v => v.Value, v => new StockQuantity(v));
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
