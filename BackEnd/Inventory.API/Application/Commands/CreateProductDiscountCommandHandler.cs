using MediatR;
using Inventory.API.Application.DTOs;
using Inventory.API.Domain.Aggregates;
using Inventory.API.Domain.Repositories;

namespace Inventory.API.Application.Commands;

public class CreateProductDiscountCommandHandler : IRequestHandler<CreateProductDiscountCommand, (ProductDiscount? Discount, string? Error)>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductDiscountRepository _productDiscountRepository;

    public CreateProductDiscountCommandHandler(
        IProductRepository productRepository,
        IProductDiscountRepository productDiscountRepository)
    {
        _productRepository = productRepository;
        _productDiscountRepository = productDiscountRepository;
    }

    public async Task<(ProductDiscount? Discount, string? Error)> Handle(CreateProductDiscountCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var product = await _productRepository.GetByIdAsync(req.ProductId, cancellationToken);
        if (product == null)
            return (null, "Ürün bulunamadı.");

        try
        {
            var discount = ProductDiscount.Create(
                req.ProductId,
                req.DiscountPercent,
                req.StartAt,
                req.EndAt,
                req.Name);
            _productDiscountRepository.Add(discount);
            await _productDiscountRepository.SaveChangesAsync(cancellationToken);
            return (discount, null);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message);
        }
    }
}
