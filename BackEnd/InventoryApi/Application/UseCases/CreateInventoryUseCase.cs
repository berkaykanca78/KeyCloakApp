using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;
using InventoryApi.Domain.ValueObjects;

namespace InventoryApi.Application.UseCases;

public class CreateInventoryUseCase
{
    private readonly IInventoryRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;

    public CreateInventoryUseCase(
        IInventoryRepository repository,
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
    }

    public async Task<InventoryItem?> ExecuteAsync(Guid productId, Guid warehouseId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken);
        if (product == null || warehouse == null)
            return null;

        var item = InventoryItem.Create(productId, warehouseId, new StockQuantity(quantity));
        _repository.Add(item);
        await _repository.SaveChangesAsync(cancellationToken);
        return item;
    }
}
