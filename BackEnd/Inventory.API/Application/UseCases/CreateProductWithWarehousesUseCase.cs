using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;
using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Application.UseCases;

/// <summary>
/// Ürün oluşturur ve seçilen her depo için stok kalemi açar (deposu olmayan ürün olamaz).
/// </summary>
public class CreateProductWithWarehousesUseCase
{
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IInventoryRepository _inventoryRepository;

    public CreateProductWithWarehousesUseCase(
        IProductRepository productRepository,
        IWarehouseRepository warehouseRepository,
        IInventoryRepository inventoryRepository)
    {
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<(Product? Product, string? Error)> ExecuteAsync(
        string name,
        decimal unitPrice,
        string currency,
        string? imageKey,
        IReadOnlyList<Guid> warehouseIds,
        int initialQuantity,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return (null, "Ürün adı boş olamaz.");
        if (warehouseIds == null || warehouseIds.Count == 0)
            return (null, "En az bir depo seçmelisiniz. Deposu olmayan ürün eklenemez.");

        if (initialQuantity < 0)
            initialQuantity = 0;
        if (string.IsNullOrWhiteSpace(currency)) currency = "TRY";

        var product = Product.Create(name.Trim(), unitPrice, currency, imageKey);
        _productRepository.Add(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        var quantity = new StockQuantity(initialQuantity);
        foreach (var warehouseId in warehouseIds.Distinct())
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken);
            if (warehouse == null)
                continue;
            var item = InventoryItem.Create(product.Id, warehouseId, quantity);
            _inventoryRepository.Add(item);
        }

        await _inventoryRepository.SaveChangesAsync(cancellationToken);
        return (product, null);
    }
}
