using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;

namespace OrderApi.Application.UseCases;

/// <summary>
/// Application use case: Giriş yapan kullanıcının siparişlerini getirir.
/// </summary>
public class GetMyOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GetMyOrdersUseCase(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<Order>> ExecuteAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Array.Empty<Order>();
        return await _orderRepository.GetByCreatedByAsync(username, cancellationToken);
    }
}
