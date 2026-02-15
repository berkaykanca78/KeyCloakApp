using MediatR;
using Basket.API.Domain.Aggregates;
using Basket.API.Domain.Repositories;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command Handler: Sepete ürün ekler veya miktarını artırır.
/// </summary>
public class AddBasketItemCommandHandler : IRequestHandler<AddBasketItemCommand, CustomerBasketCommandResult>
{
    private readonly IBasketRepository _repository;

    public AddBasketItemCommandHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerBasketCommandResult> Handle(AddBasketItemCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ProductId) || request.Quantity < 1)
            return CustomerBasketCommandResult.Fail("ProductId ve Quantity (>=1) gerekli.");
        try
        {
            var basket = await _repository.GetByBuyerIdAsync(request.BuyerId, cancellationToken)
                ?? CustomerBasket.Create(request.BuyerId);
            basket.AddItem(
                request.ProductId,
                request.ProductName,
                request.Quantity,
                request.InventoryItemId,
                request.UnitPrice,
                request.Currency);
            await _repository.SaveAsync(basket, cancellationToken);
            return CustomerBasketCommandResult.Ok(basket);
        }
        catch (ArgumentException ex)
        {
            return CustomerBasketCommandResult.Fail(ex.Message);
        }
    }
}
