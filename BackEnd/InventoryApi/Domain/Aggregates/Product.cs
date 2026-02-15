namespace InventoryApi.Domain.Aggregates;

/// <summary>
/// Ürün master — ad ve resim bilgisi.
/// </summary>
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>S3/MinIO'da saklanan ürün resmi object key.</summary>
    public string? ImageKey { get; private set; }

    private Product() { }

    public static Product Create(string name, string? imageKey = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            ImageKey = imageKey
        };
    }

    public void SetImageKey(string? imageKey) => ImageKey = imageKey;
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ürün adı boş olamaz.", nameof(name));
        Name = name.Trim();
    }
}
