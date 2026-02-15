using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Application.DTOs;
using Ordering.API.Application.Ports;
using Ordering.API.Domain.Aggregates;
using Shared.Api;

namespace Ordering.API.Presentation.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        var (message, time) = _orderService.GetPublic();
        return Ok(ResultDto<object>.Success(new { Message = message, Time = time }));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Order>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _orderService.GetAllAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<Order>>.Success(list));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("my")]
    public async Task<ActionResult<ResultDto<IEnumerable<Order>>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? string.Empty;
        var list = await _orderService.GetMyOrdersAsync(username, cancellationToken);
        return Ok(ResultDto<IEnumerable<Order>>.Success(list));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Order>>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? "unknown";
        var result = await _orderService.CreateOrderAsync(request.CustomerId, request.ProductId, request.Quantity, request.UnitPrice, username, cancellationToken);
        if (!result.Success)
            return BadRequest(ResultDto<Order>.Failure(result.ErrorMessage ?? "Sipariş oluşturulamadı."));
        return CreatedAtAction(nameof(GetAll), new { id = result.Order!.Id }, ResultDto<Order>.Success(result.Order!, "Sipariş oluşturuldu."));
    }
}
