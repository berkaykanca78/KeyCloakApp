namespace InventoryApi.Domain.Aggregates;

/// <summary>
/// Ürün master — ad, resim ve fiyat bilgisi.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>S3/MinIO'da saklanan ürün resmi object key.</summary>
    public string? ImageKey { get; private set; }
    /// <summary>Birim fiyat (liste fiyatı).</summary>
    public decimal UnitPrice { get; private set; }
    /// <summary>Para birimi (örn. TRY, USD).</summary>
    public string Currency { get; private set; } = "TRY";

    private Product() { }

    public static Product Create(string name, decimal unitPrice = 0, string currency = "TRY", string? imageKey = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
        if (unitPrice < 0) throw new ArgumentException("Fiyat negatif olamaz.", nameof(unitPrice));
        if (string.IsNullOrWhiteSpace(currency)) currency = "TRY";
        var c = currency.Trim().ToUpperInvariant();
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ImageKey = imageKey,
            UnitPrice = unitPrice,
            Currency = c.Length >= 3 ? c[..3] : c
        };
    }

    public void SetImageKey(string? imageKey) => ImageKey = imageKey;
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
        Name = name.Trim();
    }
    public void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0) throw new ArgumentException("Fiyat negatif olamaz.", nameof(unitPrice));
        UnitPrice = unitPrice;
    }
    public void SetCurrency(string currency)
    {
        if (!string.IsNullOrWhiteSpace(currency))
            Currency = currency.Trim().ToUpperInvariant()[..Math.Min(3, currency.Trim().Length)];
    }
}
