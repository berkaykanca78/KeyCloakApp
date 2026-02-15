using Ordering.API.Application.Commands;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Ports;

/// <summary>
/// Inbound port: Sipariş uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface IOrderService
{
    (string Message, DateTime Time) GetPublic();
    Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetMyOrdersAsync(string username, CancellationToken cancellationToken = default);
    Task<CreateOrderCommandResult> CreateOrderAsync(Guid customerId, Guid productId, int quantity, decimal unitPrice, string createdBy, CancellationToken cancellationToken = default);
}
