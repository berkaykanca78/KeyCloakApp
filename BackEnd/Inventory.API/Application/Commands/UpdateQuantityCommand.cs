using MediatR;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Commands;

public record UpdateQuantityCommand(Guid Id, int Quantity) : IRequest<InventoryItem?>;
