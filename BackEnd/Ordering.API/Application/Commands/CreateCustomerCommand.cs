using MediatR;
using Ordering.API.Application.DTOs;

namespace Ordering.API.Application.Commands;

public record CreateCustomerCommand(CreateCustomerRequest Request) : IRequest<CreateCustomerCommandResult>;
