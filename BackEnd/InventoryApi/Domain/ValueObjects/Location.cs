namespace InventoryApi.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Depo / lokasyon adı.
/// </summary>
public readonly record struct Location
{
    public string Value { get; }

    public Location(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Lokasyon boş olamaz.", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("Lokasyon en fazla 100 karakter olabilir.", nameof(value));
        Value = value.Trim();
    }

    public override string ToString() => Value;
}
