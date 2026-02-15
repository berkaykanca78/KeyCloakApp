using Basket.API.Domain.Aggregates;

namespace Basket.API.Application.Commands;

/// <summary>Sepet komutlarının ortak sonuç tipi (güncel sepet veya hata).</summary>
public class CustomerBasketCommandResult
{
    public CustomerBasket? Basket { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static CustomerBasketCommandResult Ok(CustomerBasket basket) =>
        new() { Basket = basket, Success = true };

    public static CustomerBasketCommandResult Fail(string message) =>
        new() { Success = false, ErrorMessage = message };
}
