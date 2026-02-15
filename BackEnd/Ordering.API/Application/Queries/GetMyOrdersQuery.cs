using MediatR;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Queries;

/// <summary>
/// CQRS Query: Giriş yapan kullanıcının siparişleri.
/// </summary>
public record GetMyOrdersQuery(string Username) : IRequest<IReadOnlyList<Order>>;
