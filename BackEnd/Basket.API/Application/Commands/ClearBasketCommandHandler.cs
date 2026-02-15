using MediatR;
using Basket.API.Domain.Repositories;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command Handler: Sepeti tamamen temizler.
/// </summary>
public class ClearBasketCommandHandler : IRequestHandler<ClearBasketCommand, bool>
{
    private readonly IBasketRepository _repository;

    public ClearBasketCommandHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(request.BuyerId, cancellationToken);
    }
}
