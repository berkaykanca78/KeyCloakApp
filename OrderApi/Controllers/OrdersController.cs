using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using OrderApi.Data;
using OrderApi.Entities;
using OrderApi.Models;
using Shared.Events;

namespace OrderApi.Controllers;

/// <summary>
/// Sipariş API'si. Ortak Keycloak token ile erişilir. Veri MSSQL Order veritabanında (T-SQL).
/// Sipariş oluşturulunca OrderPlaced event yayımlanır; Inventory servisi stoktan düşer.
/// </summary>
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public OrdersController(OrderDbContext db, IPublishEndpoint publishEndpoint)
    {
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

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
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _db.Orders.OrderByDescending(o => o.CreatedAt).ToListAsync(cancellationToken);
        return Ok(list);
    }

    /// <summary>Giriş yapan kullanıcının siparişleri. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value;
        if (string.IsNullOrEmpty(username))
            return Ok(Array.Empty<Order>());
        var myOrders = await _db.Orders
            .Where(o => o.CreatedBy == username)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
        return Ok(myOrders);
    }

    /// <summary>Yeni sipariş oluştur. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? "unknown";
        var order = new Order
        {
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            CustomerName = request.CustomerName,
            CreatedBy = username,
            CreatedAt = DateTime.UtcNow
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        // Inventory servisine sipariş bildirimi — stoktan düşüm için event yayımla
        await _publishEndpoint.Publish(new OrderPlacedEvent
        {
            OrderId = order.Id,
            ProductName = order.ProductName,
            Quantity = order.Quantity
        }, cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
    }
}
