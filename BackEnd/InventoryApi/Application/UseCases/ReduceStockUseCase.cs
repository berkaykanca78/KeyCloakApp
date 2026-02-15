using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

/// <summary>
/// ProductId'ye göre stoktan düşüm: önce en çok stoku olan depodan düşer.
/// </summary>
public class ReduceStockUseCase
{
    private readonly IInventoryRepository _repository;
    private readonly IProductRepository _productRepository;

    public ReduceStockUseCase(IInventoryRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<ReduceStockResult> ExecuteAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        var productName = product?.Name ?? productId.ToString();

        var items = await _repository.GetByProductIdAsync(productId, cancellationToken);
        if (items.Count == 0)
            return ReduceStockResult.ProductNotFound(productId, productName);

        var total = items.Sum(i => i.Quantity);
        if (total < quantity)
            return ReduceStockResult.InsufficientStock(productId, productName, quantity, total);

        var remaining = quantity;
        foreach (var item in items)
        {
            if (remaining <= 0) break;
            var deduct = Math.Min(item.Quantity, remaining);
            item.ReduceStock(deduct);
            remaining -= deduct;
        }
        await _repository.SaveChangesAsync(cancellationToken);

        return ReduceStockResult.Updated(productId, productName, quantity, total - quantity);
    }
}

public sealed record ReduceStockResult(bool Success, Guid ProductId, string ProductName, string Reason, int Deducted, int NewQuantity)
{
    public static ReduceStockResult ProductNotFound(Guid productId, string productName)
        => new(false, productId, productName, "Ürün bulunamadı", 0, 0);

    public static ReduceStockResult InsufficientStock(Guid productId, string productName, int requested, int available)
        => new(false, productId, productName, "Yetersiz stok", 0, available);

    public static ReduceStockResult Updated(Guid productId, string productName, int deducted, int newQuantity)
        => new(true, productId, productName, "Stok güncellendi", deducted, newQuantity);
}
