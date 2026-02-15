namespace Ordering.API.Infrastructure.Services;

public record InventoryAvailabilityResponse(bool IsAvailable, Guid ProductId, string? ProductName, int AvailableQuantity, string Message);
