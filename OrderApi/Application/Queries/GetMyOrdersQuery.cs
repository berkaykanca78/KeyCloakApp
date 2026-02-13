using MediatR;
using OrderApi.Domain.Aggregates;

namespace OrderApi.Application.Queries;

/// <summary>
/// CQRS Query: Giriş yapan kullanıcının siparişleri.
/// </summary>
public record GetMyOrdersQuery(string Username) : IRequest<IReadOnlyList<Order>>;
