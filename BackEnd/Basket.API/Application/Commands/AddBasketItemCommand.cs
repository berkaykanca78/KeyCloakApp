using MediatR;
using Basket.API.Application.DTOs;

namespace Basket.API.Application.Commands;

/// <summary>
/// CQRS Command: Sepete ürün ekler veya miktarını artırır.
/// </summary>
public record AddBasketItemCommand(
    string BuyerId,
    string ProductId,
    string ProductName,
    int Quantity,
    string? InventoryItemId = null,
    decimal? UnitPrice = null,
    string? Currency = null) : IRequest<CustomerBasketCommandResult>;
