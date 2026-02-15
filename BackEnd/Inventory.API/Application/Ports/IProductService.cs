using Inventory.API.Application.DTOs;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Ports;

/// <summary>
/// Inbound port: Ürün uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(Product? Product, string? Error)> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<(ProductDiscount? Discount, string? Error)> CreateDiscountAsync(CreateProductDiscountRequest request, CancellationToken cancellationToken = default);
}
