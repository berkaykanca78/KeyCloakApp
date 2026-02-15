namespace Basket.API.Application.DTOs;

/// <summary>Müşteri sepeti API yanıtı.</summary>
public class CustomerBasketDto
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemDto> Items { get; set; } = new();
}
