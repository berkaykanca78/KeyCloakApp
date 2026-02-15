using MediatR;
using Ordering.API.Domain.Aggregates;
using Ordering.API.Domain.Repositories;

namespace Ordering.API.Application.Queries;

/// <summary>
/// CQRS Query Handler: Kullanıcının siparişlerini getirir.
/// </summary>
public class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, IReadOnlyList<Order>>
{
    private readonly IOrderRepository _orderRepository;

    public GetMyOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<Order>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return Array.Empty<Order>();
        return await _orderRepository.GetByCreatedByAsync(request.Username, cancellationToken);
    }
}
