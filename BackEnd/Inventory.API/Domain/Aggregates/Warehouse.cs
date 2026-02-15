namespace Inventory.API.Domain.Aggregates;

/// <summary>
/// Depo / lokasyon master.
/// </summary>
public class Warehouse
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    /// <summary>Plaka veya kod (opsiyonel).</summary>
    public string? Code { get; private set; }
    /// <summary>MinIO/S3'te depo resmi object key.</summary>
    public string? ImageKey { get; private set; }

    private Warehouse() { }

    public static Warehouse Create(string name, string? code = null, string? imageKey = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Depo adı boş olamaz.", nameof(name));
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            ImageKey = imageKey
        };
    }

    public void SetImageKey(string? imageKey) => ImageKey = imageKey;

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Depo adı boş olamaz.", nameof(name));
        Name = name.Trim();
    }
}
