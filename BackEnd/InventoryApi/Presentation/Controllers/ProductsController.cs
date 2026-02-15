using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApi.Application.DTOs;
using InventoryApi.Application.UseCases;
using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;
using Shared.Api;

namespace InventoryApi.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductDiscountRepository _productDiscountRepository;
    private readonly CreateProductWithWarehousesUseCase _createProductWithWarehousesUseCase;

    public ProductsController(
        IProductRepository productRepository,
        IProductDiscountRepository productDiscountRepository,
        CreateProductWithWarehousesUseCase createProductWithWarehousesUseCase)
    {
        _productRepository = productRepository;
        _productDiscountRepository = productDiscountRepository;
        _createProductWithWarehousesUseCase = createProductWithWarehousesUseCase;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Product>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _productRepository.GetAllAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<Product>>.Success(list));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Product>>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (request.WarehouseIds == null || request.WarehouseIds.Count == 0)
            return BadRequest(ResultDto<Product>.Failure("En az bir depo seçmelisiniz. Deposu olmayan ürün eklenemez."));

        var (product, error) = await _createProductWithWarehousesUseCase.ExecuteAsync(
            request.Name,
            request.UnitPrice,
            request.Currency ?? "TRY",
            request.ImageKey,
            request.WarehouseIds,
            request.InitialQuantity,
            cancellationToken);

        if (error != null)
            return BadRequest(ResultDto<Product>.Failure(error));
        if (product == null)
            return BadRequest(ResultDto<Product>.Failure("Ürün eklenemedi."));

        return Ok(ResultDto<Product>.Success(product, "Ürün eklendi."));
    }

    /// <summary>Ürün için dönemsel indirim tanımlar.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("discounts")]
    public async Task<ActionResult<ResultDto<ProductDiscount>>> CreateDiscount([FromBody] CreateProductDiscountRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            return NotFound(ResultDto<ProductDiscount>.Failure("Ürün bulunamadı."));
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
            return Ok(ResultDto<ProductDiscount>.Success(discount, "İndirim eklendi."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ResultDto<ProductDiscount>.Failure(ex.Message));
        }
    }
}
