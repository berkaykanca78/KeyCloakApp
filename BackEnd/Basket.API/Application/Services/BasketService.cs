using MediatR;
using Basket.API.Application.Commands;
using Basket.API.Application.Ports;
using Basket.API.Application.Queries;
using Basket.API.Domain.Aggregates;

namespace Basket.API.Application.Services;

/// <summary>
/// Primary adapter: Port implementasyonu. MediatR (CQRS) üzerinden domain'e yönlendirir.
/// </summary>
public class BasketService : IBasketService
{
    private readonly IMediator _mediator;

    public BasketService(IMediator mediator) => _mediator = mediator;

    public Task<CustomerBasket?> GetBasketAsync(string buyerId, CancellationToken cancellationToken = default)
        => _mediator.Send(new GetBasketQuery(buyerId), cancellationToken);

    public async Task<CustomerBasketCommandResult> AddItemAsync(string buyerId, string productId, string productName, int quantity, string? inventoryItemId = null, decimal? unitPrice = null, string? currency = null, CancellationToken cancellationToken = default)
        => await _mediator.Send(new AddBasketItemCommand(buyerId, productId, productName, quantity < 1 ? 1 : quantity, inventoryItemId, unitPrice, currency), cancellationToken);

    public async Task<CustomerBasketCommandResult> UpdateItemAsync(string buyerId, string productId, int quantity, CancellationToken cancellationToken = default)
        => await _mediator.Send(new UpdateBasketItemCommand(buyerId, productId, quantity), cancellationToken);

    public async Task<CustomerBasketCommandResult> RemoveItemAsync(string buyerId, string productId, CancellationToken cancellationToken = default)
        => await _mediator.Send(new RemoveBasketItemCommand(buyerId, productId), cancellationToken);

    public Task ClearAsync(string buyerId, CancellationToken cancellationToken = default)
        => _mediator.Send(new ClearBasketCommand(buyerId), cancellationToken);
}
