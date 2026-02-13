using MediatR;
using OrderApi.Domain.Aggregates;

namespace OrderApi.Application.Queries;

/// <summary>
/// CQRS Query: Tüm siparişler (Admin).
/// </summary>
public record GetOrdersQuery : IRequest<IReadOnlyList<Order>>;
