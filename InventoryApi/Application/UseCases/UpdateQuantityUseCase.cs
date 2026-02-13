using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;
using InventoryApi.Domain.ValueObjects;

namespace InventoryApi.Application.UseCases;

/// <summary>
/// Application use case: Stok miktarını günceller (Admin).
/// </summary>
public class UpdateQuantityUseCase
{
    private readonly IInventoryRepository _repository;

    public UpdateQuantityUseCase(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<InventoryItem?> ExecuteAsync(int id, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item == null) return null;
        item.SetQuantity(new StockQuantity(quantity));
        await _repository.SaveChangesAsync(cancellationToken);
        return item;
    }
}
