namespace Shared.Events.IntegrationEvents;

public record ReserveStockRequest
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
