using Basket.API.Application.Commands;
using Basket.API.Domain.Aggregates;

namespace Basket.API.Application.Ports;

/// <summary>
/// Inbound port: Sepet uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface IBasketService
{
    Task<CustomerBasket?> GetBasketAsync(string buyerId, CancellationToken cancellationToken = default);
    Task<CustomerBasketCommandResult> AddItemAsync(string buyerId, string productId, string productName, int quantity, string? inventoryItemId = null, decimal? unitPrice = null, string? currency = null, CancellationToken cancellationToken = default);
    Task<CustomerBasketCommandResult> UpdateItemAsync(string buyerId, string productId, int quantity, CancellationToken cancellationToken = default);
    Task<CustomerBasketCommandResult> RemoveItemAsync(string buyerId, string productId, CancellationToken cancellationToken = default);
    Task ClearAsync(string buyerId, CancellationToken cancellationToken = default);
}
