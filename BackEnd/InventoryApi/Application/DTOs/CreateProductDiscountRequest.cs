namespace InventoryApi.Application.DTOs;

/// <summary>
/// Ürün için dönemsel indirim oluşturma.
/// </summary>
public record CreateProductDiscountRequest(
    Guid ProductId,
    decimal DiscountPercent,
    DateTime StartAt,
    DateTime EndAt,
    string? Name = null);
