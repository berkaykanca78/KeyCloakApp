namespace OrderApi.Domain.Events;

/// <summary>
/// Domain Event: Sipariş verildiğinde yayımlanır (stok düşümü için integration event'e dönüştürülür).
/// </summary>
public sealed record OrderPlacedDomainEvent
{
    public int OrderId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}
