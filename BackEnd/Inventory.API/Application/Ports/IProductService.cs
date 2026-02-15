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
    /// <summary>İndirim oluşturur. Item3 (IsNotFound): true ise ürün bulunamadı, 404 dönülmeli.</summary>
    Task<(ProductDiscount? Discount, string? Error, bool IsNotFound)> CreateDiscountAsync(CreateProductDiscountRequest request, CancellationToken cancellationToken = default);
}
