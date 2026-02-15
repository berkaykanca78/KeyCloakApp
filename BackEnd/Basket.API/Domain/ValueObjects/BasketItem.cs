namespace Basket.API.Domain.ValueObjects;

/// <summary>
/// DDD Value Object: Sepetteki tek kalem.
/// </summary>
public class BasketItem
{
    public string ProductId { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? InventoryItemId { get; private set; }
    public int Quantity { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string? Currency { get; private set; }

    private BasketItem() { }

    public static BasketItem Create(string productId, string productName, int quantity,
        string? inventoryItemId = null, decimal? unitPrice = null, string? currency = null)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("ProductId gerekli.", nameof(productId));
        if (quantity < 1)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity en az 1 olmalı.");
        return new BasketItem
        {
            ProductId = productId,
            ProductName = string.IsNullOrWhiteSpace(productName) ? productId : productName,
            InventoryItemId = inventoryItemId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Currency = currency
        };
    }

    public void SetQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity negatif olamaz.");
        Quantity = quantity;
    }

    public void AddQuantity(int delta)
    {
        if (Quantity + delta < 0)
            throw new ArgumentOutOfRangeException(nameof(delta), "Toplam miktar negatif olamaz.");
        Quantity += delta;
    }
}
