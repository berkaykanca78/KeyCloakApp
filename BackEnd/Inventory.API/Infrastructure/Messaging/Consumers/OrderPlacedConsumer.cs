using MassTransit;
using MediatR;
using Shared.Events.IntegrationEvents;
using Inventory.API.Application.Commands;

namespace Inventory.API.Infrastructure.Messaging.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(IMediator mediator, ILogger<OrderPlacedConsumer> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg = context.Message;
        var result = await _mediator.Send(new ReduceStockCommand(msg.ProductId, msg.Quantity), context.CancellationToken);

        if (!result.Success)
            _logger.LogWarning("OrderPlaced: {Reason}. ProductId={ProductId}, OrderId={OrderId}",
                result.Reason, result.ProductId, msg.OrderId);
        else
            _logger.LogInformation(
                "OrderPlaced: Stok güncellendi. ProductId={ProductId}, OrderId={OrderId}, Düşülen={Deducted}, YeniStok={NewQuantity}",
                result.ProductId, msg.OrderId, result.Deducted, result.NewQuantity);
    }
}
