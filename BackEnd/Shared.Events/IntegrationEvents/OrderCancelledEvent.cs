namespace Shared.Events.IntegrationEvents;

/// <summary>
/// Saga compensation: Sipariş iptal edildi (stok yetersiz veya hata).
/// </summary>
public record OrderCancelledEvent
{
    public int OrderId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public string Reason { get; init; } = string.Empty;
}
