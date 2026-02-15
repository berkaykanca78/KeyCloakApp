using MediatR;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.UseCases;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Commands;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, (Product? Product, string? Error)>
{
    private readonly CreateProductWithWarehousesUseCase _useCase;

    public CreateProductCommandHandler(CreateProductWithWarehousesUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<(Product? Product, string? Error)> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        if (req.WarehouseIds == null || req.WarehouseIds.Count == 0)
            return (null, "En az bir depo seçmelisiniz. Deposu olmayan ürün eklenemez.");

        return await _useCase.ExecuteAsync(
            req.Name,
            req.UnitPrice,
            req.Currency ?? "TRY",
            req.ImageKey,
            req.WarehouseIds,
            req.InitialQuantity,
            cancellationToken);
    }
}
