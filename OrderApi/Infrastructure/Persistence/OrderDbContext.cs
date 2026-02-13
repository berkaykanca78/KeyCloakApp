using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Aggregates;

namespace OrderApi.Infrastructure.Persistence;

/// <summary>
/// Order veritabanı (MSSQL) — Infrastructure katmanı, Domain aggregate.
/// Transactional Outbox için: https://masstransit.io/documentation/configuration/middleware/outbox
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.ToTable("OutboxMessages");
            e.HasKey(x => x.Id);
            e.Property(x => x.MessageType).HasMaxLength(500).IsRequired();
            e.Property(x => x.Payload).IsRequired();
        });

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
