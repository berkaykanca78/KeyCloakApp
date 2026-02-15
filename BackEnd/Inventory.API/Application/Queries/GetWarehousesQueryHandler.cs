using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, IReadOnlyList<Warehouse>>
{
    private readonly IWarehouseRepository _repository;

    public GetWarehousesQueryHandler(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Warehouse>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
        => await _repository.GetAllAsync(cancellationToken);
}
