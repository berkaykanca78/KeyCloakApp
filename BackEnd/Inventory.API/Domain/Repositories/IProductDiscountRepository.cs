using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Domain.Repositories;

public interface IProductDiscountRepository
{
    Task<ProductDiscount?> GetActiveByProductIdAsync(Guid productId, DateTime? asOf = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDiscount>> GetActiveByProductIdsAsync(IEnumerable<Guid> productIds, DateTime? asOf = null, CancellationToken cancellationToken = default);
    void Add(ProductDiscount discount);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
