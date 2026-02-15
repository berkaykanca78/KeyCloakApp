namespace Inventory.API.Application.DTOs;

public record CreateWarehouseRequest(string Name, string? Code = null);
