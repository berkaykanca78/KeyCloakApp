namespace OrderApi.Application.DTOs;

public record CreateOrderRequest(Guid CustomerId, Guid ProductId, int Quantity, decimal UnitPrice);
