using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;
using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Application.Commands;

public class UpdateQuantityCommandHandler : IRequestHandler<UpdateQuantityCommand, InventoryItem?>
{
    private readonly IInventoryRepository _repository;

    public UpdateQuantityCommandHandler(IInventoryRepository repository) => _repository = repository;

    public async Task<InventoryItem?> Handle(UpdateQuantityCommand request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item == null) return null;
        item.SetQuantity(new StockQuantity(request.Quantity));
        await _repository.SaveChangesAsync(cancellationToken);
        return item;
    }
}
