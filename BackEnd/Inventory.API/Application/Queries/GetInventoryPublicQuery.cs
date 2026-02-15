using MediatR;

namespace Inventory.API.Application.Queries;

public record GetInventoryPublicQuery : IRequest<GetInventoryPublicResponse>;

public record GetInventoryPublicResponse(string Message, object Items, DateTime Time);
