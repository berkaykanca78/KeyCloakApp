using MediatR;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetInventoryPublicQueryHandler : IRequestHandler<GetInventoryPublicQuery, GetInventoryPublicResponse>
{
    private readonly IInventoryRepository _repository;
    private readonly IProductDiscountRepository _discountRepository;

    public GetInventoryPublicQueryHandler(IInventoryRepository repository, IProductDiscountRepository discountRepository)
    {
        _repository = repository;
        _discountRepository = discountRepository;
    }

    public async Task<GetInventoryPublicResponse> Handle(GetInventoryPublicQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var productIds = list.Select(i => i.ProductId).Distinct().ToList();
        var now = DateTime.UtcNow;
        var activeDiscounts = await _discountRepository.GetActiveByProductIdsAsync(productIds, now, cancellationToken);
        var discountByProduct = activeDiscounts.GroupBy(d => d.ProductId).ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.DiscountPercent.Value).First());

        var items = list.Select(i =>
        {
            var product = i.Product;
            var unitPrice = product?.UnitPrice ?? 0;
            var currency = product?.Currency ?? "TRY";
            decimal? discountPercent = null;
            decimal? priceAfterDiscount = null;
            if (product != null && discountByProduct.TryGetValue(product.Id, out var disc))
            {
                discountPercent = disc.DiscountPercent.Value;
                priceAfterDiscount = unitPrice * (1 - disc.DiscountPercent.Value / 100m);
            }
            return new
            {
                i.Id,
                i.ProductId,
                ProductName = product != null ? product.Name.Value : "",
                WarehouseName = i.Warehouse != null ? i.Warehouse.Name.Value : "",
                InStock = i.Quantity.Value > 0,
                Quantity = i.Quantity.Value,
                UnitPrice = unitPrice,
                Currency = currency,
                DiscountPercent = discountPercent,
                PriceAfterDiscount = priceAfterDiscount
            };
        }).ToList();
        return new GetInventoryPublicResponse(
            "Inventory.API - Stok servisi. Giriş yaparak detay ve güncelleme yapabilirsiniz.",
            items,
            DateTime.UtcNow);
    }
}
