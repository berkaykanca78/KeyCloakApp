namespace Ordering.API.Infrastructure.Services;

public interface IInventoryAvailabilityClient
{
    Task<(bool IsAvailable, string? Message, int AvailableQuantity)> CheckAsync(Guid productId, int quantity, CancellationToken cancellationToken = default);
}
