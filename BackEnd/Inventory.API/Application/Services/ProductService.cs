using MediatR;
using Inventory.API.Application.Commands;
using Inventory.API.Application.DTOs;
using Inventory.API.Application.Ports;
using Inventory.API.Application.Queries;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Services;

/// <summary>
/// Primary adapter: Ürün port implementasyonu. MediatR (CQRS) üzerinden handler'lara yönlendirir; handler'lar repository kullanır.
/// </summary>
public class ProductService : IProductService
{
    private readonly IMediator _mediator;

    public ProductService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => (await _mediator.Send(new GetProductsQuery(), cancellationToken)).AsEnumerable();

    public async Task<(Product? Product, string? Error)> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
        => await _mediator.Send(new CreateProductCommand(request), cancellationToken);

    public async Task<(ProductDiscount? Discount, string? Error)> CreateDiscountAsync(CreateProductDiscountRequest request, CancellationToken cancellationToken = default)
        => await _mediator.Send(new CreateProductDiscountCommand(request), cancellationToken);
}
