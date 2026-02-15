using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;

namespace OrderApi.Infrastructure.Persistence;

public class CustomerRepository : ICustomerRepository
{
    private readonly OrderDbContext _db;

    public CustomerRepository(OrderDbContext db) => _db = db;

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Customer?> GetByKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default)
        => _db.Customers.FirstOrDefaultAsync(c => c.KeycloakSub == keycloakSub, cancellationToken);

    public void Add(Customer customer) => _db.Customers.Add(customer);
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}
