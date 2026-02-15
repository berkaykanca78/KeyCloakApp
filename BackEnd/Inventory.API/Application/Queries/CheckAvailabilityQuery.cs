using MediatR;
using Inventory.API.Application.Results;

namespace Inventory.API.Application.Queries;

public record CheckAvailabilityQuery(Guid ProductId, int Quantity) : IRequest<CheckAvailabilityResult>;
