using MassTransit;
using OrderApi.Domain.Aggregates;
using OrderApi.Domain.Repositories;
using OrderApi.Domain.ValueObjects;
using Shared.Events.IntegrationEvents;

namespace OrderApi.Application.UseCases;

/// <summary>
/// Application use case: Sipariş oluşturur, kalıcılaştırır ve OrderPlaced integration event yayımlar.
/// </summary>
public class CreateOrderUseCase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderUseCase(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<(Order Order, bool Success)> ExecuteAsync(
        string productName,
        int quantity,
        string customerName,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var product = new ProductName(productName);
        var qty = new OrderQuantity(quantity);
        var customer = new CustomerName(customerName);

        var (order, _) = Order.Place(product, qty, customer, createdBy);

        _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new OrderPlacedEvent
        {
            OrderId = order.Id,
            ProductName = order.ProductName,
            Quantity = order.Quantity
        }, cancellationToken);

        return (order, true);
    }
}
