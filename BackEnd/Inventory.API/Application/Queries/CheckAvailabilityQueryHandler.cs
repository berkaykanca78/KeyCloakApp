using MediatR;
using Inventory.API.Application.Results;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class CheckAvailabilityQueryHandler : IRequestHandler<CheckAvailabilityQuery, CheckAvailabilityResult>
{
    private readonly IInventoryRepository _repository;
    private readonly IProductRepository _productRepository;

    public CheckAvailabilityQueryHandler(IInventoryRepository repository, IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<CheckAvailabilityResult> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
            return CheckAvailabilityResult.InvalidInput("Miktar 0'dan büyük olmalıdır.");

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        var productName = product?.Name ?? request.ProductId.ToString();

        var items = await _repository.GetByProductIdAsync(request.ProductId, cancellationToken);
        var totalQty = items.Sum(i => i.Quantity);

        if (items.Count == 0)
            return CheckAvailabilityResult.ProductNotFound(request.ProductId, productName);

        if (totalQty >= request.Quantity)
            return CheckAvailabilityResult.Available(request.ProductId, productName, totalQty);
        return CheckAvailabilityResult.InsufficientStock(request.ProductId, productName, request.Quantity, totalQty);
    }
}
