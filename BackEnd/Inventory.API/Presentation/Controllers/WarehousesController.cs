using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Domain.Aggregates;
using Shared.Api;

namespace Inventory.API.Presentation.Controllers;

[ApiController]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Warehouse>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _warehouseService.GetAllAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<Warehouse>>.Success(list));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Warehouse>>> Create([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.CreateAsync(request, cancellationToken);
        if (warehouse == null)
            return BadRequest(ResultDto<Warehouse>.Failure("Depo adı geçersiz veya eklenemedi."));
        return Ok(ResultDto<Warehouse>.Success(warehouse, "Depo eklendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ResultDto<object>>> UploadImage(Guid id, CancellationToken cancellationToken = default)
    {
        IFormFile? file = null;
        if (Request.HasFormContentType && Request.Form.Files.Count > 0)
            file = Request.Form.Files.GetFile("file") ?? Request.Form.Files[0];
        if (file == null || file.Length == 0)
            return BadRequest(ResultDto<object>.Failure("Dosya gelmedi. Form alan adı 'file' olmalı, Content-Type: multipart/form-data."));
        var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        var contentType = file.ContentType ?? "application/octet-stream";
        if (!allowed.Contains(contentType))
            return BadRequest(ResultDto<object>.Failure($"Sadece resim dosyaları kabul edilir. Gelen: {contentType}"));

        await using var stream = file.OpenReadStream();
        var (success, imageKey, error) = await _warehouseService.UploadImageAsync(id, stream, contentType, file.FileName, cancellationToken);
        if (!success)
        {
            if (error?.Contains("bulunamadı") == true)
                return NotFound(ResultDto<object>.Failure(error));
            return StatusCode(500, ResultDto<object>.Failure(error ?? "Yükleme hatası."));
        }
        return Ok(ResultDto<object>.Success(new { imageKey }, "Depo resmi yüklendi."));
    }

    [HttpGet("{id:guid}/image")]
    public async Task<ActionResult<ResultDto<object>>> GetImageUrl(Guid id, [FromQuery] int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var (url, error) = await _warehouseService.GetImageUrlAsync(id, expirySeconds, cancellationToken);
        if (url == null)
            return NotFound(ResultDto<object>.Failure(error ?? "Depo veya resim bulunamadı."));
        return Ok(ResultDto<object>.Success(new { url }));
    }

    [HttpGet("{id:guid}/image/stream")]
    public async Task<IActionResult> GetImageStream(Guid id, CancellationToken cancellationToken = default)
    {
        var (stream, contentType) = await _warehouseService.GetImageStreamAsync(id, cancellationToken);
        if (stream == null)
            return NotFound();
        return File(stream, contentType ?? "image/jpeg", enableRangeProcessing: true);
    }
}
