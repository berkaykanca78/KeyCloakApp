using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<Product>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        => await _repository.GetAllAsync(cancellationToken);
}
