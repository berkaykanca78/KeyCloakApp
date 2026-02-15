namespace Basket.API.Application.DTOs;

/// <summary>Sepet kalemi API yanıtı.</summary>
public class BasketItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? InventoryItemId { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Currency { get; set; }
}
