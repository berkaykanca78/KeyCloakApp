using MediatR;

namespace Ordering.API.Application.Commands;

public record CreateOrderCommand(
    Guid CustomerId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string CreatedBy) : IRequest<CreateOrderCommandResult>;
