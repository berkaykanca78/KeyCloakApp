using MediatR;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Commands;

public class ReduceStockCommandHandler : IRequestHandler<ReduceStockCommand, ReduceStockResult>
{
    private readonly IInventoryRepository _repository;
    private readonly IProductRepository _productRepository;

    public ReduceStockCommandHandler(IInventoryRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<ReduceStockResult> Handle(ReduceStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        var productName = product?.Name ?? request.ProductId.ToString();

        var items = await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);
        if (items.Count == 0)
            return ReduceStockResult.ProductNotFound(request.ProductId, productName);

        var total = items.Sum(i => i.Quantity);
        if (total < request.Quantity)
            return ReduceStockResult.InsufficientStock(request.ProductId, productName, request.Quantity, total);

        var remaining = request.Quantity;
        foreach (var item in items)
        {
            if (remaining <= 0) break;
            var deduct = Math.Min(item.Quantity, remaining);
            item.ReduceStock(deduct);
            remaining -= deduct;
        }
        await _repository.SaveChangesAsync(cancellationToken);

        return ReduceStockResult.Updated(request.ProductId, productName, request.Quantity, total - request.Quantity);
    }
}
