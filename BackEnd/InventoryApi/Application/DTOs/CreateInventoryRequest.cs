namespace InventoryApi.Application.DTOs;

/// <summary>
/// Stoka yeni kalem ekleme isteği (ProductId, WarehouseId ile).
/// </summary>
public record CreateInventoryRequest(Guid ProductId, Guid WarehouseId, int Quantity);
