using MassTransit;
using Shared.Events.IntegrationEvents;
using InventoryApi.Application.UseCases;

namespace InventoryApi.Infrastructure.Messaging.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ReduceStockUseCase _reduceStockUseCase;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ReduceStockUseCase reduceStockUseCase, ILogger<OrderPlacedConsumer> logger)
    {
        _reduceStockUseCase = reduceStockUseCase;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg = context.Message;
        var result = await _reduceStockUseCase.ExecuteAsync(msg.ProductId, msg.Quantity, context.CancellationToken);

        if (!result.Success)
            _logger.LogWarning("OrderPlaced: {Reason}. ProductId={ProductId}, OrderId={OrderId}",
                result.Reason, result.ProductId, msg.OrderId);
        else
            _logger.LogInformation(
                "OrderPlaced: Stok güncellendi. ProductId={ProductId}, OrderId={OrderId}, Düşülen={Deducted}, YeniStok={NewQuantity}",
                result.ProductId, msg.OrderId, result.Deducted, result.NewQuantity);
    }
}
