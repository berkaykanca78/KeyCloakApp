using Basket.API.Domain.Aggregates;

namespace Basket.API.Domain.Repositories;

/// <summary>
/// DDD Repository: Sepet aggregate'ının kalıcılığı (Redis).
/// </summary>
public interface IBasketRepository
{
    Task<CustomerBasket?> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default);
    Task SaveAsync(CustomerBasket basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string buyerId, CancellationToken cancellationToken = default);
}
