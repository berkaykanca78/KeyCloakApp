using Microsoft.AspNetCore.Http;
using MediatR;
using Inventory.API.Application.Commands;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Application.Queries;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Services;

/// <summary>
/// Primary adapter: Depo port implementasyonu. MediatR (CQRS) üzerinden handler'lara yönlendirir; handler'lar repository kullanır.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private static readonly string[] AllowedImageContentTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

    private readonly IMediator _mediator;

    public WarehouseService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _mediator.Send(new GetWarehousesQuery(), cancellationToken)).AsEnumerable();

    public async Task<Warehouse?> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
        => await _mediator.Send(new CreateWarehouseCommand(request), cancellationToken);

    public async Task<UploadImageResult> UploadImageFromFormAsync(Guid warehouseId, IFormFile? file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return UploadImageResult.BadRequest("Dosya gelmedi. Form alan adı 'file' olmalı, Content-Type: multipart/form-data.");
        var contentType = file.ContentType ?? "application/octet-stream";
        if (!AllowedImageContentTypes.Contains(contentType))
            return UploadImageResult.BadRequest($"Sadece resim dosyaları kabul edilir. Gelen: {contentType}");

        await using var stream = file.OpenReadStream();
        var (success, imageKey, error) = await UploadImageAsync(warehouseId, stream, contentType, file.FileName, cancellationToken);
        if (!success)
        {
            if (error?.Contains("bulunamadı") == true)
                return UploadImageResult.NotFound(error);
            return UploadImageResult.ServerError(error ?? "Yükleme hatası.");
        }
        return UploadImageResult.Ok(imageKey!);
    }

    public async Task<(bool Success, string? ImageKey, string? Error)> UploadImageAsync(Guid warehouseId, Stream fileStream, string contentType, string? fileName, CancellationToken cancellationToken = default)
        => await _mediator.Send(new UploadWarehouseImageCommand(warehouseId, fileStream, contentType, fileName), cancellationToken);

    public async Task<(string? Url, string? Error)> GetImageUrlAsync(Guid warehouseId, int expirySeconds = 3600, CancellationToken cancellationToken = default)
        => await _mediator.Send(new GetWarehouseImageUrlQuery(warehouseId, expirySeconds), cancellationToken);

    public async Task<(Stream? Stream, string? ContentType)> GetImageStreamAsync(Guid warehouseId, CancellationToken cancellationToken = default)
        => await _mediator.Send(new GetWarehouseImageStreamQuery(warehouseId), cancellationToken);
}
