using System.Text.Json;
using MediatR;
using OrderApi.Domain.Aggregates;
using OrderApi.Domain.ValueObjects;
using OrderApi.Infrastructure.Persistence;
using OrderApi.Infrastructure.Services;
using Shared.Events.IntegrationEvents;

namespace OrderApi.Application.Commands;

/// <summary>
/// CQRS Command Handler: Stok yeterliliği kontrolü, sipariş oluşturma, Outbox'a event yazma.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderCommandResult>
{
    private readonly OrderDbContext _dbContext;
    private readonly IInventoryAvailabilityClient _availabilityClient;

    public CreateOrderCommandHandler(OrderDbContext dbContext, IInventoryAvailabilityClient availabilityClient)
    {
        _dbContext = dbContext;
        _availabilityClient = availabilityClient;
    }

    public async Task<CreateOrderCommandResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        (bool isAvailable, string? message, int _) result;
        try
        {
            result = await _availabilityClient.CheckAsync(request.ProductName, request.Quantity, cancellationToken);
        }
        catch (Exception)
        {
            return new CreateOrderCommandResult(null, false, "Stok servisi şu an kullanılamıyor. Lütfen daha sonra tekrar deneyin.");
        }
        if (!result.isAvailable)
            return new CreateOrderCommandResult(null, false, result.message ?? "Yetersiz stok.");

        var product = new ProductName(request.ProductName);
        var qty = new OrderQuantity(request.Quantity);
        var customer = new CustomerName(request.CustomerName);

        var (order, _) = Order.Place(product, qty, customer, request.CreatedBy);

        var correlationId = Guid.NewGuid();
        var evt = new OrderPlacedEvent
        {
            CorrelationId = correlationId,
            OrderId = 0,
            ProductName = order.ProductName,
            Quantity = order.Quantity
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        evt = evt with { OrderId = order.Id };
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = typeof(OrderPlacedEvent).FullName!,
            Payload = JsonSerializer.Serialize(evt),
            CreatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderCommandResult(order, true);
    }
}
