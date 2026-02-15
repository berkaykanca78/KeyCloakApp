using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Domain.Repositories;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Warehouse warehouse);
    Task<bool> UpdateImageKeyAsync(Guid id, string imageKey, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
