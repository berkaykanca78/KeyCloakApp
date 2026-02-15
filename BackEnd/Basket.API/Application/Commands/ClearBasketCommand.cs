using MediatR;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command: Sepeti tamamen temizler.
/// </summary>
public record ClearBasketCommand(string BuyerId) : IRequest<bool>;
