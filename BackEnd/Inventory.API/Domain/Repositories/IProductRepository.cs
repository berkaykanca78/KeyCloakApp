using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Product product);
    Task<bool> UpdateImageKeyAsync(Guid productId, string imageKey, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
