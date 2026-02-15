using Basket.API.Domain.ValueObjects;

namespace Basket.API.Domain.Aggregates;

/// <summary>
/// DDD Aggregate Root: Müşteri sepeti (anonim veya giriş yapmış kullanıcı).
/// </summary>
public class CustomerBasket
{
    public string BuyerId { get; private set; } = string.Empty;
    private readonly List<BasketItem> _items = new();
    public IReadOnlyList<BasketItem> Items => _items.AsReadOnly();

    private CustomerBasket() { }

    public static CustomerBasket Create(string buyerId)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
            throw new ArgumentException("BuyerId gerekli.", nameof(buyerId));
        return new CustomerBasket { BuyerId = buyerId };
    }

    public void AddItem(string productId, string productName, int quantity,
        string? inventoryItemId = null, decimal? unitPrice = null, string? currency = null)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
            existing.AddQuantity(quantity);
        else
            _items.Add(BasketItem.Create(productId, productName, quantity, inventoryItemId, unitPrice, currency));
    }

    public bool UpdateQuantity(string productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return false;
        if (quantity <= 0)
        {
            _items.Remove(item);
            return true;
        }
        item.SetQuantity(quantity);
        return true;
    }

    public bool RemoveItem(string productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return false;
        _items.Remove(item);
        return true;
    }

    public void Clear()
    {
        _items.Clear();
    }
}
