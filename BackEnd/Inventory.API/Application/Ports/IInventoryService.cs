using Inventory.API.Application.Queries;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Ports;

/// <summary>
/// Inbound port: Envanter uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface IInventoryService
{
    Task<GetInventoryPublicResponse> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CheckAvailabilityResult> CheckAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
    Task<InventoryItem?> UpdateQuantityAsync(Guid id, int quantity, CancellationToken cancellationToken = default);
    Task<InventoryItem?> CreateAsync(Guid productId, Guid warehouseId, int quantity, CancellationToken cancellationToken = default);
    Task<(bool Success, string? ImageKey, string? Error)> UploadImageAsync(Guid inventoryItemId, Stream fileStream, string contentType, string? fileName, CancellationToken cancellationToken = default);
    Task<(string? Url, string? Error)> GetImageUrlAsync(Guid inventoryItemId, int expirySeconds = 3600, CancellationToken cancellationToken = default);
    Task<(Stream? Stream, string? ContentType)> GetImageStreamAsync(Guid inventoryItemId, CancellationToken cancellationToken = default);
}
