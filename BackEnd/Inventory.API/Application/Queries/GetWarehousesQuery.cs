using MediatR;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Queries;

public record GetWarehousesQuery : IRequest<IReadOnlyList<Warehouse>>;
