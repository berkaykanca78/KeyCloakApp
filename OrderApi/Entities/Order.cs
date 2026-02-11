namespace OrderApi.Entities;

/// <summary>
/// Sipariş. MSSQL Order veritabanında saklanır (T-SQL).
/// </summary>
public class Order
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
