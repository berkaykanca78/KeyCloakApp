using MassTransit;
using Shared.Events.IntegrationEvents;
using InventoryApi.Application.UseCases;

namespace InventoryApi.Infrastructure.Messaging.Consumers;

public class ReserveStockConsumer : IConsumer<ReserveStockRequest>
{
    private readonly ReduceStockUseCase _reduceStockUseCase;
    private readonly ILogger<ReserveStockConsumer> _logger;

    public ReserveStockConsumer(ReduceStockUseCase reduceStockUseCase, ILogger<ReserveStockConsumer> logger)
    {
        _reduceStockUseCase = reduceStockUseCase;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReserveStockRequest> context)
    {
        var msg = context.Message;
        var result = await _reduceStockUseCase.ExecuteAsync(msg.ProductId, msg.Quantity, context.CancellationToken);

        await context.RespondAsync(new ReserveStockResponse
        {
            CorrelationId = msg.CorrelationId,
            OrderId = msg.OrderId,
            Success = result.Success,
            Reason = result.Success ? null : result.Reason
        });

        if (result.Success)
            _logger.LogInformation("ReserveStock: OrderId={OrderId}, ProductId={ProductId}, Deducted={Deducted}",
                msg.OrderId, result.ProductId, result.Deducted);
        else
            _logger.LogWarning("ReserveStock failed: OrderId={OrderId}, Reason={Reason}", msg.OrderId, result.Reason);
    }
}
