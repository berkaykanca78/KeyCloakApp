using Microsoft.EntityFrameworkCore;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Infrastructure.Persistence;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _db;

    public InventoryRepository(InventoryDbContext db) => _db = db;

    public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.InventoryItems.Include(i => i.Product).Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<InventoryItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var list = await _db.InventoryItems.Include(i => i.Product).Include(i => i.Warehouse)
            .Where(i => i.ProductId == productId)
            .OrderByDescending(i => i.Quantity)
            .ToListAsync(cancellationToken);
        return list;
    }

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.InventoryItems.Include(i => i.Product).Include(i => i.Warehouse)
            .OrderBy(i => i.Product!.Name).ThenBy(i => i.Warehouse!.Name)
            .ToListAsync(cancellationToken);
        return list;
    }

    public void Add(InventoryItem item) => _db.InventoryItems.Add(item);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
