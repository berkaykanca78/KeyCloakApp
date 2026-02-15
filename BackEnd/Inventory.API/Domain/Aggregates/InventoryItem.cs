using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Domain.Aggregates;

/// <summary>
/// DDD Aggregate Root: Stok kalemi. Product ve Warehouse ile ilişkili; Guid PK. Ürün resmi Product.ImageKey'de tutulur.
/// </summary>
public class InventoryItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public StockQuantity Quantity { get; private set; }

    public Product? Product { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    private InventoryItem() { }

    public static InventoryItem Create(Guid productId, Guid warehouseId, StockQuantity quantity)
    {
        return new InventoryItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity
        };
    }

    public int ReduceStock(int amount)
    {
        if (amount <= 0) return 0;
        var deducted = Math.Min(Quantity.Value, amount);
        var newQuantity = Math.Max(0, Quantity.Value - amount);
        SetQuantity(new StockQuantity(newQuantity));
        return deducted;
    }

    public void SetQuantity(StockQuantity quantity) => Quantity = quantity;
}
