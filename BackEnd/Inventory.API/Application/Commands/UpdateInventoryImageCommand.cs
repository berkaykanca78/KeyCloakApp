using MediatR;

namespace Inventory.API.Application.Commands;

public record UpdateInventoryImageCommand(Guid InventoryId, string ImageKey) : IRequest<bool>;
