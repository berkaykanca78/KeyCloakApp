using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.UseCases;

public class CreateProductUseCase
{
    private readonly IProductRepository _repository;

    public CreateProductUseCase(IProductRepository repository) => _repository = repository;

    public async Task<Product?> ExecuteAsync(string name, decimal unitPrice = 0, string currency = "TRY", string? imageKey = null, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(name, unitPrice, currency, imageKey);
        _repository.Add(product);
        await _repository.SaveChangesAsync(cancellationToken);
        return product;
    }
}
