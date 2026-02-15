using MediatR;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;
using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Application.Commands;

public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, InventoryItem?>
{
    private readonly IInventoryRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public CreateInventoryCommandHandler(
        IInventoryRepository repository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<InventoryItem?> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (product == null || warehouse == null)
            return null;

        var item = InventoryItem.Create(request.ProductId, request.WarehouseId, new StockQuantity(request.Quantity));
        _repository.Add(item);
        await _repository.SaveChangesAsync(cancellationToken);
        return item;
    }
}
