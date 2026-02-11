namespace OrderApi.Models;

public record CreateOrderRequest(string ProductName, int Quantity, string CustomerName);
