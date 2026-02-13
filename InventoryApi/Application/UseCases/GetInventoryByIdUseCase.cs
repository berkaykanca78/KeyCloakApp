using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

/// <summary>
/// Application use case: Tek ürün stok detayı.
/// </summary>
public class GetInventoryByIdUseCase
{
    private readonly IInventoryRepository _repository;

    public GetInventoryByIdUseCase(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryItem?> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }
}
