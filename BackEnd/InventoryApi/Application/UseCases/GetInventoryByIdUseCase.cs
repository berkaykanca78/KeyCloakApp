using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

public class GetInventoryByIdUseCase
{
    private readonly IInventoryRepository _repository;

    public GetInventoryByIdUseCase(IInventoryRepository repository) => _repository = repository;

    public async Task<InventoryItem?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
        => await _repository.GetByIdAsync(id, cancellationToken);
}
