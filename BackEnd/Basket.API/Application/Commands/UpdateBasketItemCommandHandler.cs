using MediatR;
using Basket.API.Domain.Repositories;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command Handler: Sepetteki ürün miktarını günceller.
/// </summary>
public class UpdateBasketItemCommandHandler : IRequestHandler<UpdateBasketItemCommand, CustomerBasketCommandResult>
{
    private readonly IBasketRepository _repository;

    public UpdateBasketItemCommandHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerBasketCommandResult> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await _repository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
        if (basket == null)
            return CustomerBasketCommandResult.Fail("Sepet bulunamadı.");
        var updated = basket.UpdateQuantity(request.ProductId, request.Quantity);
        if (!updated)
            return CustomerBasketCommandResult.Fail("Ürün sepette yok.");
        await _repository.SaveAsync(basket, cancellationToken);
        return CustomerBasketCommandResult.Ok(basket);
    }
}
