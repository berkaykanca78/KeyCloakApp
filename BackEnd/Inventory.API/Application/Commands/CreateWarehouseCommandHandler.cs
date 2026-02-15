using MediatR;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.UseCases;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Commands;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Warehouse?>
{
    private readonly CreateWarehouseUseCase _useCase;

    public CreateWarehouseCommandHandler(CreateWarehouseUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task<Warehouse?> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var req = request.Request;
            return await _useCase.ExecuteAsync(req.Name, req.Code, cancellationToken);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
