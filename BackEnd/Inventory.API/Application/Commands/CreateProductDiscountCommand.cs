using MediatR;
using Inventory.API.Application.DTOs;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Commands;

public record CreateProductDiscountCommand(CreateProductDiscountRequest Request) : IRequest<(ProductDiscount? Discount, string? Error)>;
