using MassTransit;
using Shared.Events.IntegrationEvents;

namespace OrderApi.Application.Saga;

public class OrderStateMachine : MassTransitStateMachine<OrderSagaState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderPlaced);
        Request(() => ReserveStock, r => r.Timeout = TimeSpan.FromSeconds(30));

        Initially(
            When(OrderPlaced)
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.ProductId = context.Message.ProductId;
                    context.Saga.Quantity = context.Message.Quantity;
                })
                .Request(ReserveStock, context => new ReserveStockRequest
                {
                    CorrelationId = context.Message.CorrelationId,
                    OrderId = context.Message.OrderId,
                    ProductId = context.Message.ProductId,
                    Quantity = context.Message.Quantity
                })
                .TransitionTo(ReserveStock.Pending)
        );

        During(ReserveStock.Pending,
            When(ReserveStock.Completed)
                .ThenAsync(async context =>
                {
                    if (!context.Message.Success)
                        await context.Publish(new OrderCancelledEvent
                        {
                            OrderId = context.Saga.OrderId,
                            ProductId = context.Saga.ProductId,
                            Quantity = context.Saga.Quantity,
                            Reason = context.Message.Reason ?? "Stok rezervasyonu başarısız"
                        });
                })
                .Finalize(),

            When(ReserveStock.Faulted)
                .PublishAsync(context => context.Init<OrderCancelledEvent>(new OrderCancelledEvent
                {
                    OrderId = context.Saga.OrderId,
                    ProductId = context.Saga.ProductId,
                    Quantity = context.Saga.Quantity,
                    Reason = "Stok servisi hatası"
                }))
                .Finalize(),

            When(ReserveStock.TimeoutExpired)
                .PublishAsync(context => context.Init<OrderCancelledEvent>(new OrderCancelledEvent
                {
                    OrderId = context.Saga.OrderId,
                    ProductId = context.Saga.ProductId,
                    Quantity = context.Saga.Quantity,
                    Reason = "Stok rezervasyonu zaman aşımı"
                }))
                .Finalize()
        );
    }

    public Event<OrderPlacedEvent> OrderPlaced { get; private set; } = null!;
    public Request<OrderSagaState, ReserveStockRequest, ReserveStockResponse> ReserveStock { get; private set; } = null!;
}
