using MediatR;
using Inventory.API.Application.Contracts;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Queries;

public class GetWarehouseImageUrlQueryHandler : IRequestHandler<GetWarehouseImageUrlQuery, (string? Url, string? Error)>
{
    private readonly IWarehouseRepository _repository;
    private readonly IStorageService _storage;

    public GetWarehouseImageUrlQueryHandler(IWarehouseRepository repository, IStorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<(string? Url, string? Error)> Handle(GetWarehouseImageUrlQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse == null || string.IsNullOrEmpty(warehouse.ImageKey))
            return (null, "Depo veya resim bulunamadı.");
        var url = await _storage.GetDownloadUrlAsync(warehouse.ImageKey, request.ExpirySeconds, cancellationToken);
        return (url, null);
    }
}
