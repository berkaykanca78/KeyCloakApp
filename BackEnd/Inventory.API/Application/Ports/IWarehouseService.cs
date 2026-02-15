using Microsoft.AspNetCore.Http;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Ports;

/// <summary>
/// Inbound port: Depo uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface IWarehouseService
{
    Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Warehouse?> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ImageKey, string? Error)> UploadImageAsync(Guid warehouseId, Stream fileStream, string contentType, string? fileName, CancellationToken cancellationToken = default);
    /// <summary>Form dosyasını doğrular ve depo resmini yükler.</summary>
    Task<UploadImageResult> UploadImageFromFormAsync(Guid warehouseId, IFormFile? file, CancellationToken cancellationToken = default);
    Task<(string? Url, string? Error)> GetImageUrlAsync(Guid warehouseId, int expirySeconds = 3600, CancellationToken cancellationToken = default);
    Task<(Stream? Stream, string? ContentType)> GetImageStreamAsync(Guid warehouseId, CancellationToken cancellationToken = default);
}
