using Ordering.API.Domain.ValueObjects;

namespace Ordering.API.Domain.Aggregates;

/// <summary>
/// DDD Aggregate Root: Sipariş. Guid PK; CustomerId ve ProductId FK. UnitPrice sipariş anındaki birim fiyat.
/// </summary>
public class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    /// <summary>Sipariş anındaki birim fiyat (indirimli veya liste fiyatı).</summary>
    public decimal UnitPrice { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public Customer? Customer { get; private set; }

    private Order() { }

    public static (Order Order, Domain.Events.OrderPlacedDomainEvent DomainEvent) Place(Guid customerId, Guid productId, OrderQuantity quantity, decimal unitPrice, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            createdBy = "unknown";
        if (unitPrice < 0) unitPrice = 0;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            ProductId = productId,
            Quantity = quantity.Value,
            UnitPrice = unitPrice,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        var domainEvent = new Domain.Events.OrderPlacedDomainEvent
        {
            OrderId = order.Id,
            ProductId = productId,
            Quantity = quantity.Value
        };

        return (order, domainEvent);
    }
}
