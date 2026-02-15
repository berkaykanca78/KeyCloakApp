namespace Basket.API.Application.DTOs;

/// <summary>Sepette miktar güncelleme isteği (PUT /api/basket/items/{productId}).</summary>
public class UpdateBasketItemRequest
{
    public int Quantity { get; set; }
}
