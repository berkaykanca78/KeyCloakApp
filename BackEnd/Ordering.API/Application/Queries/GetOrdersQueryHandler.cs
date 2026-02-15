using MediatR;
using Ordering.API.Domain.Aggregates;
using Ordering.API.Domain.Repositories;

namespace Ordering.API.Application.Queries;

/// <summary>
/// CQRS Query Handler: Tüm siparişleri getirir.
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<Order>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _orderRepository.GetAllAsync(cancellationToken);
    }
}
