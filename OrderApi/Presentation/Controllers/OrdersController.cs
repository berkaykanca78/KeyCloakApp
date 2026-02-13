using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Commands;
using OrderApi.Application.DTOs;
using OrderApi.Application.Queries;
using OrderApi.Domain.Aggregates;
using Shared.Api;

namespace OrderApi.Presentation.Controllers;

/// <summary>
/// Sipariş API'si. CQRS (MediatR) + Saga/Outbox. Tüm yanıtlar ResultDto ile sarılı.
/// </summary>
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Token gerekmez; genel bilgi.</summary>
    [HttpGet("public")]
    public IActionResult GetPublic() => Ok(ResultDto<object>.Success(new
    {
        Message = "OrderApi - Sipariş servisi (CQRS + Saga/Outbox). Giriş yaparak sipariş oluşturabilir veya listeyi görebilirsiniz.",
        Time = DateTime.UtcNow
    }));

    /// <summary>Tüm siparişler. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Order>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new GetOrdersQuery(), cancellationToken);
        return Ok(ResultDto<IEnumerable<Order>>.Success(list));
    }

    /// <summary>Giriş yapan kullanıcının siparişleri. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("my")]
    public async Task<ActionResult<ResultDto<IEnumerable<Order>>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? string.Empty;
        var list = await _mediator.Send(new GetMyOrdersQuery(username), cancellationToken);
        return Ok(ResultDto<IEnumerable<Order>>.Success(list));
    }

    /// <summary>Yeni sipariş oluştur. Stok yeterliliği baştan kontrol edilir; yetersizse hata döner.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Order>>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? "unknown";
        try
        {
            var result = await _mediator.Send(new CreateOrderCommand(
                request.ProductName,
                request.Quantity,
                request.CustomerName,
                username), cancellationToken);
            if (!result.Success)
                return BadRequest(ResultDto<Order>.Failure(result.ErrorMessage ?? "Sipariş oluşturulamadı."));
            return CreatedAtAction(nameof(GetAll), new { id = result.Order!.Id }, ResultDto<Order>.Success(result.Order!, "Sipariş oluşturuldu."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ResultDto<Order>.Failure(ex.Message));
        }
    }
}
