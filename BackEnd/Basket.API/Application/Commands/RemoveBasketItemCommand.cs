using MediatR;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command: Sepetten ürün kaldırır.
/// </summary>
public record RemoveBasketItemCommand(string BuyerId, string ProductId) : IRequest<CustomerBasketCommandResult>;
