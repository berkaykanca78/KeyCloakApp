using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetAllInventoryQueryHandler : IRequestHandler<GetAllInventoryQuery, IReadOnlyList<InventoryItem>>
{
    private readonly IInventoryRepository _repository;

    public GetAllInventoryQueryHandler(IInventoryRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<InventoryItem>> Handle(GetAllInventoryQuery request, CancellationToken cancellationToken)
        => await _repository.GetAllAsync(cancellationToken);
}
