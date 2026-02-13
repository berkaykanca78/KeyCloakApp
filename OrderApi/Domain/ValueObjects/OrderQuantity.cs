namespace OrderApi.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Sipariş miktarı (pozitif tam sayı).
/// </summary>
public readonly record struct OrderQuantity
{
    public int Value { get; }

    public OrderQuantity(int value)
    {
        if (value < 1)
            throw new ArgumentException("Miktar en az 1 olmalıdır.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value.ToString();
}
