using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

/// <summary>
/// Sipariş öncesi stok yeterliliği kontrolü (OrderApi bu endpoint'i çağırır).
/// </summary>
public class CheckAvailabilityUseCase
{
    private readonly IInventoryRepository _repository;

    public CheckAvailabilityUseCase(IInventoryRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Ürün adına göre istenen miktarın stokta olup olmadığını kontrol eder.
    /// </summary>
    public async Task<CheckAvailabilityResult> ExecuteAsync(string productName, int quantity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return CheckAvailabilityResult.InvalidInput("Ürün adı boş olamaz.");
        if (quantity <= 0)
            return CheckAvailabilityResult.InvalidInput("Miktar 0'dan büyük olmalıdır.");

        var item = await _repository.GetByProductNameAsync(productName, cancellationToken);
        if (item == null)
            return CheckAvailabilityResult.ProductNotFound(productName);

        if (item.Quantity >= quantity)
            return CheckAvailabilityResult.Available(productName, item.Quantity);
        return CheckAvailabilityResult.InsufficientStock(productName, quantity, item.Quantity);
    }
}

/// <summary>
/// Stok yeterliliği sonucu.
/// </summary>
public record CheckAvailabilityResult(bool IsAvailable, string ProductName, string Message, int RequestedQuantity, int AvailableQuantity)
{
    public static CheckAvailabilityResult Available(string productName, int availableQuantity)
        => new(true, productName, "Stok yeterli.", 0, availableQuantity);

    public static CheckAvailabilityResult InsufficientStock(string productName, int requested, int available)
        => new(false, productName, "Yetersiz stok.", requested, available);

    public static CheckAvailabilityResult ProductNotFound(string productName)
        => new(false, productName, "Ürün bulunamadı.", 0, 0);

    public static CheckAvailabilityResult InvalidInput(string message)
        => new(false, "", message, 0, 0);
}
