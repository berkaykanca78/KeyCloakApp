using MediatR;

namespace Inventory.API.Application.Queries;

public record GetWarehouseImageStreamQuery(Guid WarehouseId) : IRequest<(Stream? Stream, string? ContentType)>;
