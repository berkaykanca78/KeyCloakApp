namespace OrderApi.Infrastructure.Services;

/// <summary>
/// InventoryApi /inventory/availability yanıtı (Shared.Api.ResultDto ile sarılı gelir).
/// </summary>
public record InventoryAvailabilityResponse(bool IsAvailable, string ProductName, int AvailableQuantity, string Message);
