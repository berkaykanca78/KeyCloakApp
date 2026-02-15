namespace Shared.Events.IntegrationEvents;

public record ReserveStockResponse
{
    public Guid CorrelationId { get; init; }
    public Guid OrderId { get; init; }
    public bool Success { get; init; }
    public string? Reason { get; init; }
}
