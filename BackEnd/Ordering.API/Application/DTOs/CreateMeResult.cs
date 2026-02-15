using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.DTOs;

/// <summary>CreateMe işlemi sonucu: müşteri veya hata bilgisi.</summary>
public record CreateMeResult(Customer? Customer, bool AlreadyExisted, bool Unauthorized, string? ServerErrorMessage)
{
    public static CreateMeResult FromUnauthorized() => new(null, false, true, null);
    public static CreateMeResult ServerError(string message) => new(null, false, false, message);
    public static CreateMeResult Success(Customer customer, bool alreadyExisted) => new(customer, alreadyExisted, false, null);
}
