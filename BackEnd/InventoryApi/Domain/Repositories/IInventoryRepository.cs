using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Domain.Repositories;

/// <summary>
/// DDD Repository interface: Stok aggregate'ine kalıcı erişim.
/// </summary>
public interface IInventoryRepository
{
    Task<InventoryItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByProductNameAsync(string productName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(InventoryItem item);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
