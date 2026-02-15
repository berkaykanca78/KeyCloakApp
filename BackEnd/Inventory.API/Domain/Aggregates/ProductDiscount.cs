namespace Inventory.API.Domain.Aggregates;

/// <summary>
/// Ürün için dönemsel indirim. Belirli tarih aralığında geçerli.
/// </summary>
public class ProductDiscount
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    /// <summary>İndirim yüzdesi (0-100).</summary>
    public decimal DiscountPercent { get; private set; }
    /// <summary>İndirim adı (örn. "Yaz İndirimi").</summary>
    public string? Name { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }

    public Product? Product { get; private set; }

    private ProductDiscount() { }

    public static ProductDiscount Create(Guid productId, decimal discountPercent, DateTime startAt, DateTime endAt, string? name = null)
    {
        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("İndirim yüzdesi 0-100 arasında olmalıdır.", nameof(discountPercent));
        if (endAt < startAt)
            throw new ArgumentException("Bitiş tarihi başlangıçtan önce olamaz.", nameof(endAt));
        return new ProductDiscount
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            DiscountPercent = discountPercent,
            Name = name?.Trim(),
            StartAt = startAt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startAt, DateTimeKind.Utc) : startAt.ToUniversalTime(),
            EndAt = endAt.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endAt, DateTimeKind.Utc) : endAt.ToUniversalTime()
        };
    }

    /// <summary>Verdiğiniz tarihte bu indirim geçerli mi?</summary>
    public bool IsActiveAt(DateTime at)
    {
        var t = at.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(at, DateTimeKind.Utc) : at.ToUniversalTime();
        return t >= StartAt && t <= EndAt;
    }
}
