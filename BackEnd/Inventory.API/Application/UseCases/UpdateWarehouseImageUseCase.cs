using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.UseCases;

public class UpdateWarehouseImageUseCase
{
    private readonly IWarehouseRepository _repository;

    public UpdateWarehouseImageUseCase(IWarehouseRepository repository) => _repository = repository;

    public async Task<bool> ExecuteAsync(Guid warehouseId, string imageKey, CancellationToken cancellationToken = default)
        => await _repository.UpdateImageKeyAsync(warehouseId, imageKey, cancellationToken);
}
