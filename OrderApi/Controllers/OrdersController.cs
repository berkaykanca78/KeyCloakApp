using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OrderApi.Controllers;

/// <summary>
/// Sipariş API'si. Ortak Keycloak token ile erişilir.
/// </summary>
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private static readonly List<Order> _orders = new()
    {
        new Order(1, "Ürün A", 3, "Müşteri 1", "user", DateTime.UtcNow.AddDays(-2)),
        new Order(2, "Ürün B", 1, "Müşteri 2", "admin", DateTime.UtcNow.AddDays(-1)),
        new Order(3, "Ürün C", 5, "Müşteri 3", "user", DateTime.UtcNow),
    };
    private static int _nextId = 4;

    /// <summary>Token gerekmez; genel bilgi.</summary>
    [HttpGet("public")]
    public IActionResult GetPublic() => Ok(new
    {
        Message = "OrderApi - Sipariş servisi. Giriş yaparak sipariş oluşturabilir veya listeyi görebilirsiniz.",
        Time = DateTime.UtcNow
    });

    /// <summary>Tüm siparişler. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetAll()
    {
        return Ok(_orders.OrderByDescending(o => o.CreatedAt));
    }

    /// <summary>Giriş yapan kullanıcının siparişleri. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("my")]
    public IActionResult GetMyOrders()
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value;
        var myOrders = _orders.Where(o => string.Equals(o.CreatedBy, username, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(myOrders);
    }

    /// <summary>Yeni sipariş oluştur. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderRequest request)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? "unknown";
        var order = new Order(_nextId++, request.ProductName, request.Quantity, request.CustomerName, username, DateTime.UtcNow);
        _orders.Add(order);
        return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
    }
}

public record Order(int Id, string ProductName, int Quantity, string CustomerName, string CreatedBy, DateTime CreatedAt);
public record CreateOrderRequest(string ProductName, int Quantity, string CustomerName);
