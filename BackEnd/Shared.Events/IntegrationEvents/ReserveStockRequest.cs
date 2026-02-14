namespace Shared.Events.IntegrationEvents;

/// <summary>
/// Saga adımı: OrderApi saga'sı InventoryApi'den stok rezervasyonu ister.
/// </summary>
public record ReserveStockRequest
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
