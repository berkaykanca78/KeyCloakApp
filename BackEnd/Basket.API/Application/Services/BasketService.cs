using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MediatR;
using Basket.API.Application.Commands;
using Basket.API.Application.DTOs;
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

    public string ResolveBuyerId(string? queryBuyerId, ClaimsPrincipal user, IHeaderDictionary requestHeaders, Action<string, string> setResponseHeader)
    {
        var request = new ResolveBuyerIdRequest(
            queryBuyerId,
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub"),
            requestHeaders[BasketHeaders.BasketId].FirstOrDefault());
        var result = ResolveBuyerIdCore(request);
        if (result.HeaderToSet is { } header)
            setResponseHeader(header.Name, header.Value);
        return result.BuyerId;
    }

    private static ResolveBuyerIdResult ResolveBuyerIdCore(ResolveBuyerIdRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ExplicitBuyerId))
            return new ResolveBuyerIdResult(request.ExplicitBuyerId.Trim(), null);
        if (!string.IsNullOrEmpty(request.JwtSub))
            return new ResolveBuyerIdResult(request.JwtSub, null);
        if (!string.IsNullOrEmpty(request.HeaderBasketId))
            return new ResolveBuyerIdResult(request.HeaderBasketId, null);
        var newId = Guid.NewGuid().ToString("N");
        return new ResolveBuyerIdResult(newId, (BasketHeaders.BasketId, newId));
    }

    public CustomerBasketDto MapToDto(CustomerBasket basket)
    {
        return new CustomerBasketDto
        {
            BuyerId = basket.BuyerId,
            Items = basket.Items.Select(i => new BasketItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                InventoryItemId = i.InventoryItemId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Currency = i.Currency
            }).ToList()
        };
    }

    public async Task<CustomerBasketDto> GetBasketDtoAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        var basket = await _mediator.Send(new GetBasketQuery(buyerId), cancellationToken);
        return basket != null ? MapToDto(basket) : new CustomerBasketDto { BuyerId = buyerId, Items = new List<BasketItemDto>() };
    }

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
