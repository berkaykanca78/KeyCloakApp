using Microsoft.EntityFrameworkCore;
using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;

namespace InventoryApi.Infrastructure.Persistence;

/// <summary>
/// DDD Repository implementation: Inventory aggregate kalıcılığı (EF Core).
/// </summary>
public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _db;

    public InventoryRepository(InventoryDbContext db)
    {
        _db = db;
    }

    public Task<InventoryItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.InventoryItems.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<InventoryItem?> GetByProductNameAsync(string productName, CancellationToken cancellationToken = default)
        => _db.InventoryItems.FirstOrDefaultAsync(i => i.ProductName == productName, cancellationToken);

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.InventoryItems.ToListAsync(cancellationToken);

    public void Add(InventoryItem item) => _db.InventoryItems.Add(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
