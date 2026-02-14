using InventoryApi.Domain.ValueObjects;

namespace InventoryApi.Domain.Aggregates;

/// <summary>
/// DDD Aggregate Root: Stok kalemi. Stok düşümü ve güncelleme davranışı burada kapsüllenir.
/// </summary>
public class InventoryItem
{
    public int Id { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string Location { get; private set; } = string.Empty;

    // EF Core materialization
    private InventoryItem() { }

    /// <summary>
    /// Stoktan miktar düşer. Yetersiz stokta 0'a çekilir; gerçekte düşülen miktar döner.
    /// </summary>
    public int ReduceStock(int amount)
    {
        if (amount <= 0) return 0;
        var deducted = Math.Min(Quantity, amount);
        Quantity = Math.Max(0, Quantity - amount);
        return deducted;
    }

    /// <summary>
    /// Stok miktarını günceller (Admin).
    /// </summary>
    public void SetQuantity(StockQuantity quantity)
    {
        Quantity = quantity.Value;
    }

    /// <summary>
    /// Yeni stok kalemi oluşturur (factory).
    /// </summary>
    public static InventoryItem Create(ProductName productName, StockQuantity quantity, Location location)
    {
        return new InventoryItem
        {
            ProductName = productName.Value,
            Quantity = quantity.Value,
            Location = location.Value
        };
    }

    /// <summary>
    /// Migration/seed için: event yayımlanmadan oluşturulur (Id ile, HasData için).
    /// </summary>
    internal static InventoryItem CreateForSeed(int id, string productName, int quantity, string location)
    {
        return new InventoryItem
        {
            Id = id,
            ProductName = productName,
            Quantity = quantity,
            Location = location
        };
    }

    /// <summary>
    /// Uygulama başlangıcında boş DB için seed (Id atanmaz).
    /// </summary>
    internal static InventoryItem CreateForSeed(string productName, int quantity, string location)
    {
        return new InventoryItem
        {
            ProductName = productName,
            Quantity = quantity,
            Location = location
        };
    }
}
