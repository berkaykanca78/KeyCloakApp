using Microsoft.EntityFrameworkCore;
using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;

namespace InventoryApi.Infrastructure.Persistence;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly InventoryDbContext _db;

    public WarehouseRepository(InventoryDbContext db) => _db = db;

    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Warehouses.FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Warehouses.OrderBy(w => w.Name).ToListAsync(cancellationToken);

    public void Add(Warehouse warehouse) => _db.Warehouses.Add(warehouse);

    public async Task<bool> UpdateImageKeyAsync(Guid id, string imageKey, CancellationToken cancellationToken = default)
    {
        var count = await _db.Warehouses
            .Where(w => w.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(w => w.ImageKey, imageKey), cancellationToken);
        return count > 0;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
