using MassTransit;

namespace OrderApi.Application.Saga;

/// <summary>
/// Saga state: Sipariş -> Stok rezervasyonu akışı.
/// </summary>
public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = null!;

    public int OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
