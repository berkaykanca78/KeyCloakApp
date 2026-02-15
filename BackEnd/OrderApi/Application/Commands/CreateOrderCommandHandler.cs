using System.Text.Json;
using MediatR;
using OrderApi.Domain.Aggregates;
using OrderApi.Infrastructure.Persistence;
using OrderApi.Infrastructure.Services;
using Shared.Events.IntegrationEvents;

namespace OrderApi.Application.Commands;

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
            result = await _availabilityClient.CheckAsync(request.ProductId, request.Quantity, cancellationToken);
        }
        catch (Exception)
        {
            return new CreateOrderCommandResult(null, false, "Stok servisi şu an kullanılamıyor. Lütfen daha sonra tekrar deneyin.");
        }
        if (!result.isAvailable)
            return new CreateOrderCommandResult(null, false, result.message ?? "Yetersiz stok.");

        var (order, _) = Order.Place(request.CustomerId, request.ProductId, request.Quantity, request.UnitPrice, request.CreatedBy);

        var correlationId = Guid.NewGuid();
        var evt = new OrderPlacedEvent
        {
            CorrelationId = correlationId,
            OrderId = order.Id,
            ProductId = order.ProductId,
            Quantity = order.Quantity
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
