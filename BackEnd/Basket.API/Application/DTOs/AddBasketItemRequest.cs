namespace Basket.API.Application.DTOs;

/// <summary>Sepete ürün ekleme isteği (POST /api/basket/items).</summary>
public class AddBasketItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? InventoryItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public string? Currency { get; set; }
}
