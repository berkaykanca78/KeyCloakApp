using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

/// <summary>
/// Application use case: Sipariş event'i geldiğinde stoktan düşüm (OrderPlaced consumer tarafından çağrılır).
/// </summary>
public class ReduceStockUseCase
{
    private readonly IInventoryRepository _repository;

    public ReduceStockUseCase(IInventoryRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Ürün adına göre stoktan miktar düşer. Ürün yoksa false, düşüm yapıldıysa true döner.
    /// </summary>
    public async Task<ReduceStockResult> ExecuteAsync(string productName, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByProductNameAsync(productName, cancellationToken);
        if (item == null)
            return ReduceStockResult.ProductNotFound(productName);

        var deducted = item.ReduceStock(quantity);
        await _repository.SaveChangesAsync(cancellationToken);

        return deducted > 0
            ? ReduceStockResult.Updated(productName, deducted, item.Quantity)
            : ReduceStockResult.InsufficientStock(productName, quantity, 0);
    }
}

/// <summary>
/// Stok düşüm sonucu (loglama / raporlama için).
/// </summary>
public sealed record ReduceStockResult(bool Success, string ProductName, string Reason, int Deducted, int NewQuantity)
{
    public static ReduceStockResult ProductNotFound(string productName)
        => new(false, productName, "Ürün bulunamadı", 0, 0);

    public static ReduceStockResult InsufficientStock(string productName, int requested, int available)
        => new(false, productName, "Yetersiz stok", 0, available);

    public static ReduceStockResult Updated(string productName, int deducted, int newQuantity)
        => new(true, productName, "Stok güncellendi", deducted, newQuantity);
}
