namespace InventoryApi.Application.DTOs;

/// <summary>
/// Stok yeterliliği yanıtı (OrderApi'nin çağırdığı endpoint için).
/// </summary>
public record AvailabilityResponse(bool IsAvailable, string ProductName, int AvailableQuantity, string Message);
