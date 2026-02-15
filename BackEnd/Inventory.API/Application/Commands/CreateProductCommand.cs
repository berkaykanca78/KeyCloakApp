using MediatR;
using Inventory.API.Application.DTOs;
using Inventory.API.Domain.Aggregates;

namespace Inventory.API.Application.Commands;

public record CreateProductCommand(CreateProductRequest Request) : IRequest<(Product? Product, string? Error)>;
