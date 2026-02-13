using OrderApi.Domain.Aggregates;

namespace OrderApi.Domain.Repositories;

/// <summary>
/// DDD Repository interface: Sipariş aggregate'ine kalıcı erişim.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default);
    void Add(Order order);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
