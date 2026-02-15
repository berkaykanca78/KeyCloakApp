using MediatR;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Queries;

/// <summary>
/// CQRS Query: Tüm siparişler (Admin).
/// </summary>
public record GetOrdersQuery : IRequest<IReadOnlyList<Order>>;
