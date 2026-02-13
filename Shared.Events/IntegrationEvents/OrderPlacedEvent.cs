namespace Shared.Events.IntegrationEvents;

/// <summary>
/// Integration Event: Sipariş verildiğinde OrderApi tarafından yayımlanır.
/// InventoryApi bu event'i dinleyerek ilgili ürünün stoktan düşülmesini sağlar.
/// </summary>
public record OrderPlacedEvent
{
    public int OrderId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
