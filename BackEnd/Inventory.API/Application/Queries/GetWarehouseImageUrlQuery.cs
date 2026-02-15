using MediatR;

namespace Inventory.API.Application.Queries;

public record GetWarehouseImageUrlQuery(Guid WarehouseId, int ExpirySeconds = 3600) : IRequest<(string? Url, string? Error)>;
