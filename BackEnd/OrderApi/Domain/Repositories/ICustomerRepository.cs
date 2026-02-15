using OrderApi.Domain.Aggregates;

namespace OrderApi.Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByKeycloakSubAsync(string keycloakSub, CancellationToken cancellationToken = default);
    void Add(Customer customer);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
