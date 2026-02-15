using MediatR;
using Basket.API.Domain.Aggregates;

namespace Basket.API.Application.Queries;

/// <summary>
/// CQRS Query: BuyerId'ye göre sepeti getirir. Null = sepet yok (boş döndürülür).
/// </summary>
public record GetBasketQuery(string BuyerId) : IRequest<CustomerBasket?>;
