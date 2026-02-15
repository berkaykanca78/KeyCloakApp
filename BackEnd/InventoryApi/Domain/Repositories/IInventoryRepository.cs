using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Domain.Repositories;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(InventoryItem item);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
