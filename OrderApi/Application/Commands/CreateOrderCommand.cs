using MediatR;

namespace OrderApi.Application.Commands;

/// <summary>
/// CQRS Command: Sipariş oluşturma.
/// </summary>
public record CreateOrderCommand(
    string ProductName,
    int Quantity,
    string CustomerName,
    string CreatedBy) : IRequest<CreateOrderCommandResult>;
