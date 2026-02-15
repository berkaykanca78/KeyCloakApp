namespace Basket.API.Application.DTOs;

/// <summary>BuyerId çözümleme girişi: query param, JWT sub, header değeri.</summary>
public record ResolveBuyerIdRequest(
    string? ExplicitBuyerId,
    string? JwtSub,
    string? HeaderBasketId);
