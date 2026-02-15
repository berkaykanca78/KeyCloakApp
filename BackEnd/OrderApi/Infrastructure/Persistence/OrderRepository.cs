using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;

namespace OrderApi.Infrastructure.Persistence;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;

    public OrderRepository(OrderDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Orders.Include(o => o.Customer).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Orders.Include(o => o.Customer).OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> GetByCreatedByAsync(string createdBy, CancellationToken cancellationToken = default)
        => await _db.Orders.Include(o => o.Customer)
            .Where(o => o.CreatedBy == createdBy)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Order order) => _db.Orders.Add(order);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
