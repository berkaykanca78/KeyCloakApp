using MediatR;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command: Sepetteki ürün miktarını günceller. 0 ise satır kaldırılır.
/// </summary>
public record UpdateBasketItemCommand(string BuyerId, string ProductId, int Quantity) : IRequest<CustomerBasketCommandResult>;
