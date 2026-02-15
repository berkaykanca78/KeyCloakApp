using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Product product);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
