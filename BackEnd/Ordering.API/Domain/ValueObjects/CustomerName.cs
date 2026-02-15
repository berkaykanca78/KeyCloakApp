namespace Ordering.API.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Siparişi veren müşteri adı.
/// </summary>
public readonly record struct CustomerName
{
    public string Value { get; }

    public CustomerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Müşteri adı boş olamaz.", nameof(value));
        if (value.Length > 200)
            throw new ArgumentException("Müşteri adı en fazla 200 karakter olabilir.", nameof(value));
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
