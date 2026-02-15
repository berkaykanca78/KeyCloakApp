using MediatR;
using Inventory.API.Application.Contracts;
using Inventory.API.Application.UseCases;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Commands;

public class UploadWarehouseImageCommandHandler : IRequestHandler<UploadWarehouseImageCommand, (bool Success, string? ImageKey, string? Error)>
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IStorageService _storage;
    private readonly UpdateWarehouseImageUseCase _updateImageUseCase;

    public UploadWarehouseImageCommandHandler(
        IWarehouseRepository warehouseRepository,
        IStorageService storage,
        UpdateWarehouseImageUseCase updateImageUseCase)
    {
        _warehouseRepository = warehouseRepository;
        _storage = storage;
        _updateImageUseCase = updateImageUseCase;
    }

    public async Task<(bool Success, string? ImageKey, string? Error)> Handle(UploadWarehouseImageCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse == null)
            return (false, null, "Depo bulunamadı.");

        var ext = string.IsNullOrEmpty(request.FileName) || string.IsNullOrEmpty(Path.GetExtension(request.FileName)) ? ".jpg" : Path.GetExtension(request.FileName);
        var key = $"warehouses/{request.WarehouseId:N}/{Guid.NewGuid():N}{ext}";

        try
        {
            await _storage.UploadAsync(key, request.FileStream, request.ContentType, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, null, $"MinIO/S3 yükleme hatası: {ex.Message}");
        }

        var updated = await _updateImageUseCase.ExecuteAsync(request.WarehouseId, key, cancellationToken);
        if (!updated)
            return (false, key, "Resim S3'e yüklendi ama veritabanına ImageKey yazılamadı.");
        return (true, key, null);
    }
}
