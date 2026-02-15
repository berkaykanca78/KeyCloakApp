using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.UseCases;

public class CheckAvailabilityUseCase
{
    private readonly IInventoryRepository _repository;
    private readonly IProductRepository _productRepository;

    public CheckAvailabilityUseCase(IInventoryRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    /// <summary>
    /// ProductId'ye göre toplam stok miktarını kontrol eder (tüm depolardaki toplam).
    /// </summary>
    public async Task<CheckAvailabilityResult> ExecuteAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            return CheckAvailabilityResult.InvalidInput("Miktar 0'dan büyük olmalıdır.");

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        var productName = product != null ? product.Name.Value : productId.ToString();

        var items = await _repository.GetByProductIdAsync(productId, cancellationToken);
        var totalQty = items.Sum(i => i.Quantity.Value);

        if (items.Count == 0)
            return CheckAvailabilityResult.ProductNotFound(productId, productName);

        if (totalQty >= quantity)
            return CheckAvailabilityResult.Available(productId, productName, totalQty);
        return CheckAvailabilityResult.InsufficientStock(productId, productName, quantity, totalQty);
    }
}

public record CheckAvailabilityResult(bool IsAvailable, Guid ProductId, string ProductName, string Message, int RequestedQuantity, int AvailableQuantity)
{
    public static CheckAvailabilityResult Available(Guid productId, string productName, int availableQuantity)
        => new(true, productId, productName, "Stok yeterli.", 0, availableQuantity);

    public static CheckAvailabilityResult InsufficientStock(Guid productId, string productName, int requested, int available)
        => new(false, productId, productName, "Yetersiz stok.", requested, available);

    public static CheckAvailabilityResult ProductNotFound(Guid productId, string productName)
        => new(false, productId, productName, "Ürün bulunamadı.", 0, 0);

    public static CheckAvailabilityResult InvalidInput(string message)
        => new(false, Guid.Empty, "", message, 0, 0);
}
