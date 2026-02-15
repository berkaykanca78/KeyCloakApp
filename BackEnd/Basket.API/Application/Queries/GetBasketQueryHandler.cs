using MediatR;
using Basket.API.Domain.Aggregates;
using Basket.API.Domain.Repositories;

namespace Basket.API.Application.Queries;

/// <summary>
/// CQRS Query Handler: Sepeti repository'den getirir.
/// </summary>
public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, CustomerBasket?>
{
    private readonly IBasketRepository _repository;

    public GetBasketQueryHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerBasket?> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByBuyerIdAsync(request.BuyerId, cancellationToken);
    }
}
