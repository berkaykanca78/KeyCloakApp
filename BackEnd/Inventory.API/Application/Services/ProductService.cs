using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Application.UseCases;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Services;

/// <summary>
/// Primary adapter: Ürün port implementasyonu. Use case ve repository (outbound port) üzerinden yönlendirir.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductDiscountRepository _productDiscountRepository;
    private readonly CreateProductWithWarehousesUseCase _createProductWithWarehousesUseCase;

    public ProductService(
        IProductRepository productRepository,
        IProductDiscountRepository productDiscountRepository,
        CreateProductWithWarehousesUseCase createProductWithWarehousesUseCase)
    {
        _productRepository = productRepository;
        _productDiscountRepository = productDiscountRepository;
        _createProductWithWarehousesUseCase = createProductWithWarehousesUseCase;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => (IEnumerable<Product>)await _productRepository.GetAllAsync(cancellationToken);

    public async Task<(Product? Product, string? Error)> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (request.WarehouseIds == null || request.WarehouseIds.Count == 0)
            return (null, "En az bir depo seçmelisiniz. Deposu olmayan ürün eklenemez.");

        return await _createProductWithWarehousesUseCase.ExecuteAsync(
            request.Name,
            request.UnitPrice,
            request.Currency ?? "TRY",
            request.ImageKey,
            request.WarehouseIds,
            request.InitialQuantity,
            cancellationToken);
    }

    public async Task<(ProductDiscount? Discount, string? Error)> CreateDiscountAsync(CreateProductDiscountRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return (null, "Ürün bulunamadı.");

        try
        {
            var discount = ProductDiscount.Create(
                request.ProductId,
                request.DiscountPercent,
                request.StartAt,
                request.EndAt,
                request.Name);
            _productDiscountRepository.Add(discount);
            await _productDiscountRepository.SaveChangesAsync(cancellationToken);
            return (discount, null);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message);
        }
    }
}
