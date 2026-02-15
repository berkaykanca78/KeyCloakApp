using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.DTOs;

/// <summary>GetMe işlemi sonucu: müşteri veya hata bilgisi.</summary>
public record GetMeResult(Customer? Customer, bool Unauthorized, string? NotFoundMessage)
{
    public static GetMeResult FromUnauthorized() => new(null, true, null);
    public static GetMeResult NotFound(string message) => new(null, false, message);
    public static GetMeResult Success(Customer customer) => new(customer, false, null);
}
