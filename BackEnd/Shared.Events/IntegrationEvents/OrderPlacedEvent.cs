namespace Shared.Events.IntegrationEvents;

/// <summary>
/// Integration Event: Sipariş verildiğinde OrderApi tarafından yayımlanır.
/// Saga korelasyonu için CorrelationId kullanılır.
/// </summary>
public record OrderPlacedEvent
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
