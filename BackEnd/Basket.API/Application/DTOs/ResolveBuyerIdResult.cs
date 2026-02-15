namespace Basket.API.Application.DTOs;

/// <summary>BuyerId çözümleme sonucu. Yeni Guid üretildiyse HeaderToSet dolu olur (response'a eklenmeli).</summary>
public record ResolveBuyerIdResult(string BuyerId, (string Name, string Value)? HeaderToSet);

/// <summary>Sepet HTTP header adı (anonim sepet kimliği).</summary>
public static class BasketHeaders
{
    public const string BasketId = "X-Basket-Id";
}
