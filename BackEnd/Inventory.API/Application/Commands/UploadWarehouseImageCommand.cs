using MediatR;

namespace Inventory.API.Application.Commands;

public record UploadWarehouseImageCommand(
    Guid WarehouseId,
    Stream FileStream,
    string ContentType,
    string? FileName) : IRequest<(bool Success, string? ImageKey, string? Error)>;
