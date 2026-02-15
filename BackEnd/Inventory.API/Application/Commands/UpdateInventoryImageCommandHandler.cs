using MediatR;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Commands;

public class UpdateInventoryImageCommandHandler : IRequestHandler<UpdateInventoryImageCommand, bool>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public UpdateInventoryImageCommandHandler(IInventoryRepository inventoryRepository, IProductRepository productRepository)
    {
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public async Task<bool> Handle(UpdateInventoryImageCommand request, CancellationToken cancellationToken)
    {
        var item = await _inventoryRepository.GetByIdAsync(request.InventoryId, cancellationToken);
        if (item == null) return false;
        return await _productRepository.UpdateImageKeyAsync(item.ProductId, request.ImageKey, cancellationToken);
    }
}
