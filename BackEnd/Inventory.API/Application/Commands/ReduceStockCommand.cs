using MediatR;
using Inventory.API.Application.Results;

namespace Inventory.API.Application.Commands;

public record ReduceStockCommand(Guid ProductId, int Quantity) : IRequest<ReduceStockResult>;
