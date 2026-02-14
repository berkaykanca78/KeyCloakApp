using MassTransit;
using Shared.Events.IntegrationEvents;
using InventoryApi.Application.UseCases;

namespace InventoryApi.Infrastructure.Messaging.Consumers;

/// <summary>
/// DDD Infrastructure (Messaging): OrderPlaced integration event'ini dinler; stok düşümü application use case üzerinden yapılır.
/// </summary>
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
        var result = await _reduceStockUseCase.ExecuteAsync(
            msg.ProductName,
            msg.Quantity,
            context.CancellationToken);

        if (!result.Success)
            _logger.LogWarning("OrderPlaced: {Reason}. ProductName={ProductName}, OrderId={OrderId}",
                result.Reason, result.ProductName, msg.OrderId);
        else
            _logger.LogInformation(
                "OrderPlaced: Stok güncellendi. ProductName={ProductName}, OrderId={OrderId}, Düşülen={Deducted}, YeniStok={NewQuantity}",
                result.ProductName, msg.OrderId, result.Deducted, result.NewQuantity);
    }
}
