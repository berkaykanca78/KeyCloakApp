using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

public class GetInventoryPublicUseCase
{
    private readonly IInventoryRepository _repository;
    private readonly IProductDiscountRepository _discountRepository;

    public GetInventoryPublicUseCase(IInventoryRepository repository, IProductDiscountRepository discountRepository)
    {
        _repository = repository;
        _discountRepository = discountRepository;
    }

    public async Task<(string Message, object Items, DateTime Time)> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var productIds = list.Select(i => i.ProductId).Distinct().ToList();
        var now = DateTime.UtcNow;
        var activeDiscounts = await _discountRepository.GetActiveByProductIdsAsync(productIds, now, cancellationToken);
        var discountByProduct = activeDiscounts.GroupBy(d => d.ProductId).ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.DiscountPercent).First());

        var items = list.Select(i =>
        {
            var product = i.Product;
            var unitPrice = product?.UnitPrice ?? 0;
            var currency = product?.Currency ?? "TRY";
            decimal? discountPercent = null;
            decimal? priceAfterDiscount = null;
            if (product != null && discountByProduct.TryGetValue(product.Id, out var disc))
            {
                discountPercent = disc.DiscountPercent;
                priceAfterDiscount = unitPrice * (1 - disc.DiscountPercent / 100m);
            }
            return new
            {
                i.Id,
                i.ProductId,
                ProductName = product?.Name ?? "",
                WarehouseName = i.Warehouse?.Name ?? "",
                InStock = i.Quantity > 0,
                i.Quantity,
                UnitPrice = unitPrice,
                Currency = currency,
                DiscountPercent = discountPercent,
                PriceAfterDiscount = priceAfterDiscount
            };
        }).ToList();
        return (
            "InventoryApi - Stok servisi. Giriş yaparak detay ve güncelleme yapabilirsiniz.",
            items,
            DateTime.UtcNow
        );
    }
}
