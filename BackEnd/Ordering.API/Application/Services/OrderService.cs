using MediatR;
using Ordering.API.Application.Commands;
using Ordering.API.Application.Ports;
using Ordering.API.Application.Queries;
using Ordering.API.Domain.Aggregates;

namespace Ordering.API.Application.Services;

/// <summary>
/// Primary adapter: Sipariş port implementasyonu. MediatR üzerinden yönlendirir.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IMediator _mediator;

    public OrderService(IMediator mediator) => _mediator = mediator;

    public (string Message, DateTime Time) GetPublic()
        => ("Ordering.API - Sipariş servisi (CQRS + Saga/Outbox). Giriş yaparak sipariş oluşturabilir veya listeyi görebilirsiniz.", DateTime.UtcNow);

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
        => (IEnumerable<Order>)await _mediator.Send(new GetOrdersQuery(), cancellationToken);

    public async Task<IEnumerable<Order>> GetMyOrdersAsync(string username, CancellationToken cancellationToken = default)
        => (IEnumerable<Order>)await _mediator.Send(new GetMyOrdersQuery(username), cancellationToken);

    public async Task<CreateOrderCommandResult> CreateOrderAsync(Guid customerId, Guid productId, int quantity, decimal unitPrice, string createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _mediator.Send(new CreateOrderCommand(customerId, productId, quantity, unitPrice, createdBy), cancellationToken);
        }
        catch (ArgumentException)
        {
            return new CreateOrderCommandResult(null, false, "Geçersiz sipariş parametreleri.");
        }
    }
}
