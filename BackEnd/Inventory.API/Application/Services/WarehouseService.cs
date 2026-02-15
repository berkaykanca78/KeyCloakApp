using Inventory.API.Application.Contracts;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Application.UseCases;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Services;

/// <summary>
/// Primary adapter: Depo port implementasyonu. Use case, repository ve IStorageService (outbound port) üzerinden yönlendirir.
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly CreateWarehouseUseCase _createWarehouseUseCase;
    private readonly UpdateWarehouseImageUseCase _updateWarehouseImageUseCase;
    private readonly IStorageService _storage;

    public WarehouseService(
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

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
        => (IEnumerable<Warehouse>)await _warehouseRepository.GetAllAsync(cancellationToken);

    public async Task<Warehouse?> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _createWarehouseUseCase.ExecuteAsync(request.Name, request.Code, cancellationToken);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public async Task<(bool Success, string? ImageKey, string? Error)> UploadImageAsync(Guid warehouseId, Stream fileStream, string contentType, string? fileName, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse == null)
            return (false, null, "Depo bulunamadı.");

        var ext = string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(Path.GetExtension(fileName)) ? ".jpg" : Path.GetExtension(fileName);
        var key = $"warehouses/{warehouseId:N}/{Guid.NewGuid():N}{ext}";

        try
        {
            await _storage.UploadAsync(key, fileStream, contentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, null, $"MinIO/S3 yükleme hatası: {ex.Message}");
        }

        var updated = await _updateWarehouseImageUseCase.ExecuteAsync(warehouseId, key, cancellationToken);
        if (!updated)
            return (false, key, "Resim S3'e yüklendi ama veritabanına ImageKey yazılamadı.");
        return (true, key, null);
    }

    public async Task<(string? Url, string? Error)> GetImageUrlAsync(Guid warehouseId, int expirySeconds = 3600, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse == null || string.IsNullOrEmpty(warehouse.ImageKey))
            return (null, "Depo veya resim bulunamadı.");
        var url = await _storage.GetDownloadUrlAsync(warehouse.ImageKey, expirySeconds, cancellationToken);
        return (url, null);
    }

    public async Task<(Stream? Stream, string? ContentType)> GetImageStreamAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse == null || string.IsNullOrEmpty(warehouse.ImageKey))
            return (null, null);
        return await _storage.GetWithContentTypeAsync(warehouse.ImageKey, cancellationToken);
    }
}
