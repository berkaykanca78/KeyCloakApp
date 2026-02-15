using MediatR;

namespace OrderApi.Application.Commands;

public record CreateOrderCommand(
    Guid CustomerId,
    Guid ProductId,
    int Quantity,
    string CreatedBy) : IRequest<CreateOrderCommandResult>;
