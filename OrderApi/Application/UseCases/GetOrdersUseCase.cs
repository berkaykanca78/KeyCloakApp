using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;

namespace OrderApi.Application.UseCases;

/// <summary>
/// Application use case: Tüm siparişleri getirir (Admin).
/// </summary>
public class GetOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersUseCase(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<Order>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await _orderRepository.GetAllAsync(cancellationToken);
    }
}
