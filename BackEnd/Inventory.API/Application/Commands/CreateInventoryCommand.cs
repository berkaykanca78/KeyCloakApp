using MediatR;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Commands;

public record CreateInventoryCommand(Guid ProductId, Guid WarehouseId, int Quantity) : IRequest<InventoryItem?>;
