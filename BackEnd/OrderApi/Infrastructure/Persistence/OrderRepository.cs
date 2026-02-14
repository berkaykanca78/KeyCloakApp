using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;

namespace OrderApi.Infrastructure.Persistence;

/// <summary>
/// DDD Repository implementation: Order aggregate kalıcılığı (EF Core).
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;

    public OrderRepository(OrderDbContext db)
    {
        _db = db;
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default)
        => await _db.Orders
            .Where(o => o.CreatedBy == createdBy)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Order order) => _db.Orders.Add(order);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);
}
