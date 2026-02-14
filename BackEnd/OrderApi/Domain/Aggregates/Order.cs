using OrderApi.Domain.ValueObjects;

namespace OrderApi.Domain.Aggregates;

/// <summary>
/// DDD Aggregate Root: Sipariş. Sipariş verme davranışı burada kapsüllenir.
/// </summary>
public class Order
{
    public int Id { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    // EF Core materialization
    private Order() { }

    /// <summary>
    /// Yeni sipariş verir (factory). Domain kuralları burada uygulanır.
    /// </summary>
    public static (Order Order, Domain.Events.OrderPlacedDomainEvent DomainEvent) Place(
        ProductName productName,
        OrderQuantity quantity,
        CustomerName customerName,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            createdBy = "unknown";

        var order = new Order
        {
            ProductName = productName.Value,
            Quantity = quantity.Value,
            CustomerName = customerName.Value,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        var domainEvent = new Domain.Events.OrderPlacedDomainEvent
        {
            OrderId = 0, // Id, SaveChanges sonrası atanacak; use case güncelleyecek
            ProductName = order.ProductName,
            Quantity = order.Quantity
        };

        return (order, domainEvent);
    }

    /// <summary>
    /// Migration/seed için: event yayımlanmadan order oluşturur.
    /// </summary>
    internal static Order CreateForSeed(string productName, int quantity, string customerName, string createdBy, DateTime createdAt)
    {
        return new Order
        {
            ProductName = productName,
            Quantity = quantity,
            CustomerName = customerName,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };
    }
}
