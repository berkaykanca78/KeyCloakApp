namespace Inventory.API.Application.Results;

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
