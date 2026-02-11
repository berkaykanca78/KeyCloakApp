namespace InventoryApi.Entities;

/// <summary>
/// Envanter/stok kalemi. PostgreSQL Inventory veritabanında saklanır.
/// </summary>
public class InventoryItem
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Location { get; set; } = string.Empty;
}
