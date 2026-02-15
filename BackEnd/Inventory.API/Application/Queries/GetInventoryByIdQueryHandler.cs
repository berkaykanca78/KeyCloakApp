using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetInventoryByIdQueryHandler : IRequestHandler<GetInventoryByIdQuery, InventoryItem?>
{
    private readonly IInventoryRepository _repository;

    public GetInventoryByIdQueryHandler(IInventoryRepository repository) => _repository = repository;

    public async Task<InventoryItem?> Handle(GetInventoryByIdQuery request, CancellationToken cancellationToken)
        => await _repository.GetByIdAsync(request.Id, cancellationToken);
}
