namespace Ordering.API.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Siparişi veren müşteri adı.
/// </summary>
public readonly record struct CustomerName
{
    public string Value { get; }

    public CustomerName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        if (Value.Length > 200)
            throw new ArgumentException("Müşteri adı en fazla 200 karakter olabilir.", nameof(value));
    }

    public override string ToString() => Value;
}
