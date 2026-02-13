using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTOs;
using OrderApi.Application.UseCases;
using OrderApi.Domain.Aggregates;

namespace OrderApi.Presentation.Controllers;

/// <summary>
/// Sipariş API'si. DDD Presentation: Controller sadece use case'leri çağırır.
/// </summary>
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly GetOrdersUseCase _getOrdersUseCase;
    private readonly GetMyOrdersUseCase _getMyOrdersUseCase;
    private readonly CreateOrderUseCase _createOrderUseCase;

    public OrdersController(
        GetOrdersUseCase getOrdersUseCase,
        GetMyOrdersUseCase getMyOrdersUseCase,
        CreateOrderUseCase createOrderUseCase)
    {
        _getOrdersUseCase = getOrdersUseCase;
        _getMyOrdersUseCase = getMyOrdersUseCase;
        _createOrderUseCase = createOrderUseCase;
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
        var list = await _getOrdersUseCase.ExecuteAsync(cancellationToken);
        return Ok(list);
    }

    /// <summary>Giriş yapan kullanıcının siparişleri. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value;
        var list = await _getMyOrdersUseCase.ExecuteAsync(username ?? string.Empty, cancellationToken);
        return Ok(list);
    }

    /// <summary>Yeni sipariş oluştur. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? "unknown";
        try
        {
            var (order, _) = await _createOrderUseCase.ExecuteAsync(
                request.ProductName,
                request.Quantity,
                request.CustomerName,
                username,
                cancellationToken);
            return CreatedAtAction(nameof(GetAll), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
