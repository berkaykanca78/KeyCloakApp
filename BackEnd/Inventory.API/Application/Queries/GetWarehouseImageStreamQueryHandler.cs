using MediatR;
using Inventory.API.Application.Contracts;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetWarehouseImageStreamQueryHandler : IRequestHandler<GetWarehouseImageStreamQuery, (Stream? Stream, string? ContentType)>
{
    private readonly IWarehouseRepository _repository;
    private readonly IStorageService _storage;

    public GetWarehouseImageStreamQueryHandler(IWarehouseRepository repository, IStorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<(Stream? Stream, string? ContentType)> Handle(GetWarehouseImageStreamQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse == null || string.IsNullOrEmpty(warehouse.ImageKey))
            return (null, null);
        return await _storage.GetWithContentTypeAsync(warehouse.ImageKey, cancellationToken);
    }
}
