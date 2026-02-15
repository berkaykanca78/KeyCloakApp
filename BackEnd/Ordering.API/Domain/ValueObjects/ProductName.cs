namespace Ordering.API.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Siparişteki ürün adı.
/// </summary>
public readonly record struct ProductName
{
    public string Value { get; }

    public ProductName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Ürün adı boş olamaz.", nameof(value));
        if (value.Length > 200)
            throw new ArgumentException("Ürün adı en fazla 200 karakter olabilir.", nameof(value));
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
