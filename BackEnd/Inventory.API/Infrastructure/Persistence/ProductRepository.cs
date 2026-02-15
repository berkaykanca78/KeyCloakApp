using Microsoft.EntityFrameworkCore;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Infrastructure.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly InventoryDbContext _db;

    public ProductRepository(InventoryDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Products.OrderBy(p => p.Name.Value).ToListAsync(cancellationToken);

    public void Add(Product product) => _db.Products.Add(product);

    public async Task<bool> UpdateImageKeyAsync(Guid productId, string imageKey, CancellationToken cancellationToken = default)
    {
        var count = await _db.Products
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ImageKey, imageKey), cancellationToken);
        return count > 0;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
