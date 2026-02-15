using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

public class UpdateInventoryImageUseCase
{
    private readonly IInventoryRepository _repository;

    public UpdateInventoryImageUseCase(IInventoryRepository repository) => _repository = repository;

    public async Task<bool> ExecuteAsync(Guid inventoryId, string imageKey, CancellationToken cancellationToken = default)
        => await _repository.UpdateImageKeyAsync(inventoryId, imageKey, cancellationToken);
}
