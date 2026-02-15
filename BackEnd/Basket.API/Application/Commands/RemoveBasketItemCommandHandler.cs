using MediatR;
using Basket.API.Domain.Aggregates;
using Basket.API.Domain.Repositories;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command Handler: Sepetten ürün kaldırır.
/// </summary>
public class RemoveBasketItemCommandHandler : IRequestHandler<RemoveBasketItemCommand, CustomerBasketCommandResult>
{
    private readonly IBasketRepository _repository;

    public RemoveBasketItemCommandHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerBasketCommandResult> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await _repository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket == null)
            return CustomerBasketCommandResult.Ok(CustomerBasket.Create(request.BuyerId));
        basket.RemoveItem(request.ProductId);
        await _repository.SaveAsync(basket, cancellationToken);
        return CustomerBasketCommandResult.Ok(basket);
    }
}
