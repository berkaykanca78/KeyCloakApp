using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Aggregates;
using Shared.Api;

namespace Inventory.API.Presentation.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("public")]
    public async Task<ActionResult<ResultDto<object>>> GetPublic(CancellationToken cancellationToken)
    {
        var response = await _inventoryService.GetPublicAsync(cancellationToken);
        return Ok(ResultDto<object>.Success(new { Message = response.Message, Items = response.Items, Time = response.Time }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<InventoryItem>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _inventoryService.GetAllAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<InventoryItem>>.Success(list));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResultDto<InventoryItem>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _inventoryService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(ResultDto<InventoryItem>.Failure("Ürün bulunamadı."));
        return Ok(ResultDto<InventoryItem>.Success(item));
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ResultDto<AvailabilityResponse>>> CheckAvailability(
        [FromQuery] Guid productId,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CheckAvailabilityAsync(productId, quantity, cancellationToken);
        var response = new AvailabilityResponse(
            result.IsAvailable,
            result.ProductId,
            result.ProductName,
            result.AvailableQuantity,
            result.Message);
        if (result.IsAvailable)
            return Ok(ResultDto<AvailabilityResponse>.Success(response, result.Message));
        return BadRequest(ResultDto<AvailabilityResponse>.Failure(result.Message, new[] { $"{result.ProductName}: Mevcut stok {result.AvailableQuantity}, istenen {result.RequestedQuantity}." }));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResultDto<InventoryItem>>> UpdateQuantity(Guid id, [FromBody] UpdateQuantityRequest request, CancellationToken cancellationToken)
    {
        var item = await _inventoryService.UpdateQuantityAsync(id, request.Quantity, cancellationToken);
        if (item == null)
            return NotFound(ResultDto<InventoryItem>.Failure("Ürün bulunamadı."));
        return Ok(ResultDto<InventoryItem>.Success(item, "Stok güncellendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<InventoryItem>>> Create([FromBody] CreateInventoryRequest request, CancellationToken cancellationToken)
    {
        var item = await _inventoryService.CreateAsync(request.ProductId, request.WarehouseId, request.Quantity, cancellationToken);
        if (item == null)
            return BadRequest(ResultDto<InventoryItem>.Failure("Ürün veya depo bulunamadı. Geçerli ProductId ve WarehouseId kullanın."));
        return Ok(ResultDto<InventoryItem>.Success(item, "Stok kalemi eklendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ResultDto<object>>> UploadImage(Guid id, CancellationToken cancellationToken)
    {
        var file = Request.HasFormContentType && Request.Form.Files.Count > 0
            ? Request.Form.Files.GetFile("file") ?? Request.Form.Files[0]
            : null;
        var result = await _inventoryService.UploadImageFromFormAsync(id, file, cancellationToken);
        if (!result.Success)
            return result.ErrorKind switch
            {
                UploadImageErrorKind.NotFound => NotFound(ResultDto<object>.Failure(result.Error ?? "Bulunamadı.")),
                UploadImageErrorKind.BadRequest => BadRequest(ResultDto<object>.Failure(result.Error ?? "Geçersiz istek.")),
                _ => StatusCode(500, ResultDto<object>.Failure(result.Error ?? "Yükleme hatası."))
            };
        return Ok(ResultDto<object>.Success(new { result.ImageKey }, "Resim yüklendi (ürüne kaydedildi)."));
    }

    [HttpGet("{id:guid}/image/url")]
    public async Task<ActionResult<ResultDto<object>>> GetImageUrl(Guid id, [FromQuery] int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var (url, error) = await _inventoryService.GetImageUrlAsync(id, expirySeconds, cancellationToken);
        if (url == null)
            return NotFound(ResultDto<object>.Failure(error ?? "Ürün veya resim bulunamadı."));
        return Ok(ResultDto<object>.Success(new { url }));
    }

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken cancellationToken = default)
    {
        var (stream, contentType) = await _inventoryService.GetImageStreamAsync(id, cancellationToken);
        if (stream == null)
            return NotFound();
        return File(stream, contentType ?? "image/jpeg", enableRangeProcessing: true);
    }
}
