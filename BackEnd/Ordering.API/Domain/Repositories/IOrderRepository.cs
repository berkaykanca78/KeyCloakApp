using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default);
    void Add(Order order);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
