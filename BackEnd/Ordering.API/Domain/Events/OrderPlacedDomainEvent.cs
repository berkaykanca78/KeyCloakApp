namespace Ordering.API.Domain.Events;

public sealed record OrderPlacedDomainEvent
{
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}
