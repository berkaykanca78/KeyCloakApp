using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Domain.Aggregates;

/// <summary>
/// Depo / lokasyon master.
/// </summary>
public class Warehouse
{
    public Guid Id { get; private set; }
    public Location Name { get; private set; }
    /// <summary>Plaka veya kod (opsiyonel).</summary>
    public string? Code { get; private set; }
    /// <summary>MinIO/S3'te depo resmi object key.</summary>
    public string? ImageKey { get; private set; }

    private Warehouse() { }

    public static Warehouse Create(string name, string? code = null, string? imageKey = null)
    {
        var location = new Location(name);
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = location,
            Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim(),
            ImageKey = imageKey
        };
    }

    public void SetImageKey(string? imageKey) => ImageKey = imageKey;

    public void SetName(string name)
    {
        Name = new Location(name);
    }
}
