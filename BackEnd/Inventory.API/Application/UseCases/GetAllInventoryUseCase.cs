using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.UseCases;

/// <summary>
/// Application use case: Tüm stok listesi (Admin).
/// </summary>
public class GetAllInventoryUseCase
{
    private readonly IInventoryRepository _repository;

    public GetAllInventoryUseCase(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InventoryItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
