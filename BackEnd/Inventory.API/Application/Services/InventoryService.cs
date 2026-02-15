using Microsoft.AspNetCore.Http;
using MediatR;
using Inventory.API.Application.Commands;
using Inventory.API.Application.Contracts;
using Inventory.API.Application.Ports;
using Inventory.API.Application.Queries;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Services;

/// <summary>
/// Primary adapter: Port implementasyonu. MediatR ve IStorageService (outbound port) üzerinden yönlendirir.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IMediator _mediator;
    private readonly IStorageService _storage;

    public InventoryService(IMediator mediator, IStorageService storage)
    {
        _mediator = mediator;
        _storage = storage;
    }

    public Task<GetInventoryPublicResponse> GetPublicAsync(CancellationToken cancellationToken = default)
        => _mediator.Send(new GetInventoryPublicQuery(), cancellationToken);

    public async Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => (IEnumerable<InventoryItem>)await _mediator.Send(new GetAllInventoryQuery(), cancellationToken);

    public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _mediator.Send(new GetInventoryByIdQuery(id), cancellationToken);

    public Task<CheckAvailabilityResult> CheckAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
        => _mediator.Send(new CheckAvailabilityQuery(productId, quantity), cancellationToken);

    public async Task<InventoryItem?> UpdateQuantityAsync(Guid id, int quantity, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _mediator.Send(new UpdateQuantityCommand(id, quantity), cancellationToken);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public Task<InventoryItem?> CreateAsync(Guid productId, Guid warehouseId, int quantity, CancellationToken cancellationToken = default)
        => _mediator.Send(new CreateInventoryCommand(productId, warehouseId, quantity), cancellationToken);

    private static readonly string[] AllowedImageContentTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

    public async Task<UploadImageResult> UploadImageFromFormAsync(Guid inventoryItemId, IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return UploadImageResult.BadRequest("Dosya gelmedi. Form alan adı 'file' olmalı, Content-Type: multipart/form-data.");
        var contentType = file.ContentType ?? "application/octet-stream";
        if (!AllowedImageContentTypes.Contains(contentType))
            return UploadImageResult.BadRequest($"Sadece resim dosyaları kabul edilir. Gelen: {contentType}");

        await using var stream = file.OpenReadStream();
        var (success, imageKey, error) = await UploadImageAsync(inventoryItemId, stream, contentType, file.FileName, cancellationToken);
        if (!success)
        {
            if (error?.Contains("bulunamadı") == true)
                return UploadImageResult.NotFound(error);
            return UploadImageResult.ServerError(error ?? "Yükleme hatası.");
        }
        return UploadImageResult.Ok(imageKey!);
    }

    public async Task<(bool Success, string? ImageKey, string? Error)> UploadImageAsync(Guid inventoryItemId, Stream fileStream, string contentType, string? fileName, CancellationToken cancellationToken = default)
    {
        var item = await _mediator.Send(new GetInventoryByIdQuery(inventoryItemId), cancellationToken);
        if (item == null)
            return (false, null, "Stok kalemi bulunamadı.");

        var ext = string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(Path.GetExtension(fileName)) ? ".jpg" : Path.GetExtension(fileName);
        var key = $"products/{inventoryItemId:N}/{Guid.NewGuid():N}{ext}";

        try
        {
            await _storage.UploadAsync(key, fileStream, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, null, $"MinIO/S3 yükleme hatası: {ex.Message}");
        }

        var updated = await _mediator.Send(new UpdateInventoryImageCommand(inventoryItemId, key), cancellationToken);
        if (!updated)
            return (false, key, "Resim S3'e yüklendi ama ürün (Product) ImageKey yazılamadı.");
        return (true, key, null);
    }

    public async Task<(string? Url, string? Error)> GetImageUrlAsync(Guid inventoryItemId, int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var item = await _mediator.Send(new GetInventoryByIdQuery(inventoryItemId), cancellationToken);
        var imageKey = item?.Product?.ImageKey;
        if (item == null || string.IsNullOrEmpty(imageKey))
            return (null, "Ürün veya resim bulunamadı.");
        var url = await _storage.GetDownloadUrlAsync(imageKey, expirySeconds, cancellationToken);
        return (url, null);
    }

    public async Task<(Stream? Stream, string? ContentType)> GetImageStreamAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        var item = await _mediator.Send(new GetInventoryByIdQuery(inventoryItemId), cancellationToken);
        var imageKey = item?.Product?.ImageKey;
        if (item == null || string.IsNullOrEmpty(imageKey))
            return (null, null);
        var (stream, contentType) = await _storage.GetWithContentTypeAsync(imageKey, cancellationToken);
        return (stream, contentType);
    }
}
