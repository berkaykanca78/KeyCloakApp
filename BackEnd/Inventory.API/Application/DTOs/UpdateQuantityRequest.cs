namespace Inventory.API.Application.DTOs;

/// <summary>
/// DDD Application layer: Stok miktarı güncelleme isteği (API contract).
/// </summary>
public record UpdateQuantityRequest(int Quantity);
