namespace OrderApi.Infrastructure.Services;

/// <summary>
/// InventoryApi stok yeterliliği endpoint'ini çağırır (sipariş öncesi kontrol).
/// </summary>
public interface IInventoryAvailabilityClient
{
    Task<(bool IsAvailable, string? Message, int AvailableQuantity)> CheckAsync(string productName, int quantity, CancellationToken cancellationToken = default);
}
