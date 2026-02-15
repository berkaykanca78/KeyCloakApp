using InventoryApi.Domain.Aggregates;
using InventoryApi.Domain.Repositories;

namespace InventoryApi.Application.UseCases;

public class CreateWarehouseUseCase
{
    private readonly IWarehouseRepository _repository;

    public CreateWarehouseUseCase(IWarehouseRepository repository) => _repository = repository;

    public async Task<Warehouse?> ExecuteAsync(string name, string? code, CancellationToken cancellationToken = default)
    {
        var warehouse = Warehouse.Create(name, code);
        _repository.Add(warehouse);
        await _repository.SaveChangesAsync(cancellationToken);
        return warehouse;
    }
}
