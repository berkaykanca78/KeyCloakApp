using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

public class GetInventoryPublicUseCase
{
    private readonly IInventoryRepository _repository;

    public GetInventoryPublicUseCase(IInventoryRepository repository) => _repository = repository;

    public async Task<(string Message, object Items, DateTime Time)> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(cancellationToken);
        var items = list.Select(i => new
        {
            i.Id,
            i.ProductId,
            ProductName = i.Product?.Name ?? "",
            WarehouseName = i.Warehouse?.Name ?? "",
            InStock = i.Quantity > 0,
            i.Quantity
        }).ToList();
        return (
            "InventoryApi - Stok servisi. Giriş yaparak detay ve güncelleme yapabilirsiniz.",
            items,
            DateTime.UtcNow
        );
    }
}
