using InventoryApi.Domain.ValueObjects;

namespace InventoryApi.Domain.Aggregates;

/// <summary>
/// DDD Aggregate Root: Stok kalemi. Product ve Warehouse ile ilişkili; Guid PK.
/// </summary>
public class InventoryItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public int Quantity { get; private set; }
    /// <summary>Stok kalemi özel resim (opsiyonel); yoksa Product.ImageKey kullanılır.</summary>
    public string? ImageKey { get; private set; }

    public Product? Product { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    private InventoryItem() { }

    public static InventoryItem Create(Guid productId, Guid warehouseId, StockQuantity quantity, string? imageKey = null)
    {
        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity.Value,
            ImageKey = imageKey
        };
    }

    public int ReduceStock(int amount)
    {
        if (amount <= 0) return 0;
        var deducted = Math.Min(Quantity, amount);
        Quantity = Math.Max(0, Quantity - amount);
        return deducted;
    }

    public void SetQuantity(StockQuantity quantity) => Quantity = quantity.Value;
    public void SetImageKey(string? imageKey) => ImageKey = imageKey;
}
