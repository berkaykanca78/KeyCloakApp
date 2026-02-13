namespace Shared.Events.IntegrationEvents;

/// <summary>
/// Saga yanıtı: Stok rezervasyonu sonucu (InventoryApi -> OrderApi Saga).
/// </summary>
public record ReserveStockResponse
{
    public Guid CorrelationId { get; init; }
    public int OrderId { get; init; }
    public bool Success { get; init; }
    public string? Reason { get; init; }
}
