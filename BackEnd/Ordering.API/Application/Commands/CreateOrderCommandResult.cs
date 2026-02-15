using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Commands;

/// <summary>
/// CQRS Command sonucu: Oluşturulan sipariş veya hata.
/// </summary>
public record CreateOrderCommandResult(Order? Order, bool Success, string? ErrorMessage = null);
