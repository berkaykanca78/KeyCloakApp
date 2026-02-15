namespace Inventory.API.Application.DTOs;

/// <summary>
/// Ürün oluşturma — en az bir depo seçilmeli (deposu olmayan ürün olamaz).
/// </summary>
public record CreateProductRequest(
    string Name,
    decimal UnitPrice = 0,
    string Currency = "TRY",
    string? ImageKey = null,
    IReadOnlyList<Guid>? WarehouseIds = null,
    int InitialQuantity = 0);
