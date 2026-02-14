namespace OrderApi.Application.DTOs;

/// <summary>
/// DDD Application layer: Sipariş oluşturma isteği (API contract).
/// </summary>
public record CreateOrderRequest(string ProductName, int Quantity, string CustomerName);
