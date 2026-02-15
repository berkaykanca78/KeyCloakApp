namespace Inventory.API.Application.DTOs;

/// <summary>
/// Stok yeterliliği yanıtı (ProductId ile).
/// </summary>
public record AvailabilityResponse(bool IsAvailable, Guid ProductId, string? ProductName, int AvailableQuantity, string Message);
