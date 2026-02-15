using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Warehouse?>
{
    private readonly IWarehouseRepository _repository;

    public GetWarehouseByIdQueryHandler(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Warehouse?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(request.WarehouseId, cancellationToken);
}
