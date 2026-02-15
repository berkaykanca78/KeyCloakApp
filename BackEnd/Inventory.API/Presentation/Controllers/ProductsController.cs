using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Domain.Aggregates;
using Shared.Api;

namespace Inventory.API.Presentation.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Product>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _productService.GetAllAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<Product>>.Success(list));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Product>>> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var (product, error) = await _productService.CreateAsync(request, cancellationToken);

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
        var (discount, error) = await _productService.CreateDiscountAsync(request, cancellationToken);
        if (error != null)
        {
            if (error.Contains("bulunamadı"))
                return NotFound(ResultDto<ProductDiscount>.Failure(error));
            return BadRequest(ResultDto<ProductDiscount>.Failure(error));
        }
        if (discount == null)
            return BadRequest(ResultDto<ProductDiscount>.Failure("İndirim eklenemedi."));
        return Ok(ResultDto<ProductDiscount>.Success(discount, "İndirim eklendi."));
    }
}
