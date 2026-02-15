namespace Shared.Events.IntegrationEvents;

public record OrderCancelledEvent
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string Reason { get; init; } = string.Empty;
}
