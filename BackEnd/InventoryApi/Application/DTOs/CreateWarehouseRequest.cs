namespace InventoryApi.Application.DTOs;

public record CreateWarehouseRequest(string Name, string? Code = null);
