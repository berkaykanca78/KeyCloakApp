namespace InventoryApi.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Stok miktarı (negatif olamaz).
/// </summary>
public readonly record struct StockQuantity
{
    public int Value { get; }

    public StockQuantity(int value)
    {
        if (value < 0)
            throw new ArgumentException("Stok miktarı negatif olamaz.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value.ToString();
}
