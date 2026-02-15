namespace Inventory.API.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: İndirim yüzdesi (0-100).
/// </summary>
public readonly record struct DiscountPercent
{
    public decimal Value { get; }

    public DiscountPercent(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("İndirim yüzdesi 0-100 arasında olmalıdır.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value.ToString("F2");
}
