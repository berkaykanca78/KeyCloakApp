namespace Inventory.API.Application.Results;

public sealed record ReduceStockResult(bool Success, Guid ProductId, string ProductName, string Reason, int Deducted, int NewQuantity)
{
    public static ReduceStockResult ProductNotFound(Guid productId, string productName)
        => new(false, productId, productName, "Ürün bulunamadı", 0, 0);

    public static ReduceStockResult InsufficientStock(Guid productId, string productName, int requested, int available)
        => new(false, productId, productName, "Yetersiz stok", 0, available);

    public static ReduceStockResult Updated(Guid productId, string productName, int deducted, int newQuantity)
        => new(true, productId, productName, "Stok güncellendi", deducted, newQuantity);
}
