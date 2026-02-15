using Microsoft.EntityFrameworkCore;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Infrastructure.Persistence;

public class ProductDiscountRepository : IProductDiscountRepository
{
    private readonly InventoryDbContext _db;

    public ProductDiscountRepository(InventoryDbContext db) => _db = db;

    public async Task<ProductDiscount?> GetActiveByProductIdAsync(Guid productId, DateTime? asOf = null, CancellationToken cancellationToken = default)
    {
        var t = asOf ?? DateTime.UtcNow;
        if (t.Kind != DateTimeKind.Utc) t = t.ToUniversalTime();
        return await _db.ProductDiscounts
            .Where(d => d.ProductId == productId && d.StartAt <= t && d.EndAt >= t)
            .OrderByDescending(d => d.DiscountPercent)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductDiscount>> GetActiveByProductIdsAsync(IEnumerable<Guid> productIds, DateTime? asOf = null, CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0) return Array.Empty<ProductDiscount>();
        var t = asOf ?? DateTime.UtcNow;
        if (t.Kind != DateTimeKind.Utc) t = t.ToUniversalTime();
        return await _db.ProductDiscounts
            .Where(d => ids.Contains(d.ProductId) && d.StartAt <= t && d.EndAt >= t)
            .ToListAsync(cancellationToken);
    }

    public void Add(ProductDiscount discount) => _db.ProductDiscounts.Add(discount);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
