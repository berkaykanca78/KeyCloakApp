using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inventory.API.Application.Contracts;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.UseCases;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;
using Shared.Api;

namespace Inventory.API.Presentation.Controllers;

[ApiController]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly CreateWarehouseUseCase _createWarehouseUseCase;
    private readonly UpdateWarehouseImageUseCase _updateWarehouseImageUseCase;
    private readonly IStorageService _storage;

    public WarehousesController(
        IWarehouseRepository warehouseRepository,
        CreateWarehouseUseCase createWarehouseUseCase,
        UpdateWarehouseImageUseCase updateWarehouseImageUseCase,
        IStorageService storage)
    {
        _warehouseRepository = warehouseRepository;
        _createWarehouseUseCase = createWarehouseUseCase;
        _updateWarehouseImageUseCase = updateWarehouseImageUseCase;
        _storage = storage;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Warehouse>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _warehouseRepository.GetAllAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<Warehouse>>.Success(list));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Warehouse>>> Create([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var warehouse = await _createWarehouseUseCase.ExecuteAsync(request.Name, request.Code, cancellationToken);
            return Ok(ResultDto<Warehouse>.Success(warehouse!, "Depo eklendi."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ResultDto<Warehouse>.Failure(ex.Message));
        }
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

        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null)
            return NotFound(ResultDto<object>.Failure("Depo bulunamadı."));

        var ext = string.IsNullOrEmpty(Path.GetExtension(file.FileName)) ? ".jpg" : Path.GetExtension(file.FileName);
        var key = $"warehouses/{id:N}/{Guid.NewGuid():N}{ext}";

        try
        {
            await using var stream = file.OpenReadStream();
            await _storage.UploadAsync(key, stream, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResultDto<object>.Failure($"MinIO/S3 yükleme hatası: {ex.Message}"));
        }

        var updated = await _updateWarehouseImageUseCase.ExecuteAsync(id, key, cancellationToken);
        if (!updated)
            return StatusCode(500, ResultDto<object>.Failure("Resim S3'e yüklendi ama veritabanına ImageKey yazılamadı."));
        return Ok(ResultDto<object>.Success(new { imageKey = key }, "Depo resmi yüklendi."));
    }

    [HttpGet("{id:guid}/image")]
    public async Task<ActionResult<ResultDto<object>>> GetImageUrl(Guid id, [FromQuery] int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null || string.IsNullOrEmpty(warehouse.ImageKey))
            return NotFound(ResultDto<object>.Failure("Depo veya resim bulunamadı."));
        var url = await _storage.GetDownloadUrlAsync(warehouse.ImageKey, expirySeconds, cancellationToken);
        return Ok(ResultDto<object>.Success(new { url }));
    }

    [HttpGet("{id:guid}/image/stream")]
    public async Task<IActionResult> GetImageStream(Guid id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id, cancellationToken);
        if (warehouse == null || string.IsNullOrEmpty(warehouse.ImageKey))
            return NotFound();
        var (stream, contentType) = await _storage.GetWithContentTypeAsync(warehouse.ImageKey, cancellationToken);
        if (stream == null)
            return NotFound();
        return File(stream, contentType ?? "image/jpeg", enableRangeProcessing: true);
    }
}
