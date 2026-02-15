using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.UseCases;

/// <summary>
/// Stok kalemi (inventory) üzerinden yüklenen resmi ilgili ürüne (Product.ImageKey) yazar.
/// </summary>
public class UpdateInventoryImageUseCase
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public UpdateInventoryImageUseCase(IInventoryRepository inventoryRepository, IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public async Task<bool> ExecuteAsync(Guid inventoryId, string imageKey, CancellationToken cancellationToken = default)
    {
        var item = await _inventoryRepository.GetByIdAsync(inventoryId, cancellationToken);
        if (item == null) return false;
        return await _productRepository.UpdateImageKeyAsync(item.ProductId, imageKey, cancellationToken);
    }
}
