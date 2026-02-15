using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApi.Application.Contracts;
using InventoryApi.Application.DTOs;
using InventoryApi.Application.UseCases;
using InventoryApi.Domain.Aggregates;
using Shared.Api;

namespace InventoryApi.Presentation.Controllers;

/// <summary>Stok kalemleri (inventory items) — liste, detay, miktar güncelleme, resim.</summary>
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly GetInventoryPublicUseCase _getPublicUseCase;
    private readonly GetAllInventoryUseCase _getAllUseCase;
    private readonly GetInventoryByIdUseCase _getByIdUseCase;
    private readonly UpdateQuantityUseCase _updateQuantityUseCase;
    private readonly CheckAvailabilityUseCase _checkAvailabilityUseCase;
    private readonly CreateInventoryUseCase _createUseCase;
    private readonly UpdateInventoryImageUseCase _updateImageUseCase;
    private readonly IStorageService _storage;

    public InventoryController(
        GetInventoryPublicUseCase getPublicUseCase,
        GetAllInventoryUseCase getAllUseCase,
        GetInventoryByIdUseCase getByIdUseCase,
        UpdateQuantityUseCase updateQuantityUseCase,
        CheckAvailabilityUseCase checkAvailabilityUseCase,
        CreateInventoryUseCase createUseCase,
        UpdateInventoryImageUseCase updateImageUseCase,
        IStorageService storage)
    {
        _getPublicUseCase = getPublicUseCase;
        _getAllUseCase = getAllUseCase;
        _getByIdUseCase = getByIdUseCase;
        _updateQuantityUseCase = updateQuantityUseCase;
        _checkAvailabilityUseCase = checkAvailabilityUseCase;
        _createUseCase = createUseCase;
        _updateImageUseCase = updateImageUseCase;
        _storage = storage;
    }

    [HttpGet("public")]
    public async Task<ActionResult<ResultDto<object>>> GetPublic(CancellationToken cancellationToken)
    {
        var (message, items, time) = await _getPublicUseCase.ExecuteAsync(cancellationToken);
        return Ok(ResultDto<object>.Success(new { Message = message, Items = items, Time = time }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<InventoryItem>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _getAllUseCase.ExecuteAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<InventoryItem>>.Success(list));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResultDto<InventoryItem>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
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
        var result = await _checkAvailabilityUseCase.ExecuteAsync(productId, quantity, cancellationToken);
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
        try
        {
            var item = await _updateQuantityUseCase.ExecuteAsync(id, request.Quantity, cancellationToken);
            if (item == null)
                return NotFound(ResultDto<InventoryItem>.Failure("Ürün bulunamadı."));
            return Ok(ResultDto<InventoryItem>.Success(item, "Stok güncellendi."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ResultDto<InventoryItem>.Failure(ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<InventoryItem>>> Create([FromBody] CreateInventoryRequest request, CancellationToken cancellationToken)
    {
        var item = await _createUseCase.ExecuteAsync(request.ProductId, request.WarehouseId, request.Quantity, cancellationToken);
        if (item == null)
            return BadRequest(ResultDto<InventoryItem>.Failure("Ürün veya depo bulunamadı. Geçerli ProductId ve WarehouseId kullanın."));
        return Ok(ResultDto<InventoryItem>.Success(item, "Stok kalemi eklendi."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<ActionResult<ResultDto<object>>> UploadImage(Guid id, CancellationToken cancellationToken)
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
        var item = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        if (item == null)
            return NotFound(ResultDto<object>.Failure("Stok kalemi bulunamadı."));

        var ext = string.IsNullOrEmpty(Path.GetExtension(file.FileName)) ? ".jpg" : Path.GetExtension(file.FileName);
        var key = $"products/{id:N}/{Guid.NewGuid():N}{ext}";

        try
        {
            await using var stream = file.OpenReadStream();
            await _storage.UploadAsync(key, stream, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResultDto<object>.Failure($"MinIO/S3 yükleme hatası: {ex.Message}"));
        }

        var updated = await _updateImageUseCase.ExecuteAsync(id, key, cancellationToken);
        if (!updated)
            return StatusCode(500, ResultDto<object>.Failure("Resim S3'e yüklendi ama ürün (Product) ImageKey yazılamadı."));
        return Ok(ResultDto<object>.Success(new { imageKey = key }, "Resim yüklendi (ürüne kaydedildi)."));
    }

    [HttpGet("{id:guid}/image/url")]
    public async Task<ActionResult<ResultDto<object>>> GetImageUrl(Guid id, [FromQuery] int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var item = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        var imageKey = item?.Product?.ImageKey;
        if (item == null || string.IsNullOrEmpty(imageKey))
            return NotFound(ResultDto<object>.Failure("Ürün veya resim bulunamadı."));
        var url = await _storage.GetDownloadUrlAsync(imageKey, expirySeconds, cancellationToken);
        return Ok(ResultDto<object>.Success(new { url }));
    }

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        var imageKey = item?.Product?.ImageKey;
        if (item == null || string.IsNullOrEmpty(imageKey))
            return NotFound();
        var (stream, contentType) = await _storage.GetWithContentTypeAsync(imageKey, cancellationToken);
        if (stream == null)
            return NotFound();
        return File(stream, contentType ?? "image/jpeg", enableRangeProcessing: true);
    }
}
