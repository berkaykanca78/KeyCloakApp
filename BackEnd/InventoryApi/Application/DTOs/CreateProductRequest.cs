namespace InventoryApi.Application.DTOs;

/// <summary>
/// Ürün oluşturma — en az bir depo seçilmeli (deposu olmayan ürün olamaz).
/// </summary>
public record CreateProductRequest(
    string Name,
    string? ImageKey,
    IReadOnlyList<Guid> WarehouseIds,
    int InitialQuantity = 0);
