using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.API.Application.Commands;
using Ordering.API.Application.DTOs;
using Ordering.API.Application.Queries;
using Ordering.API.Domain.Aggregates;
using Shared.Api;

namespace Ordering.API.Presentation.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("public")]
    public IActionResult GetPublic() => Ok(ResultDto<object>.Success(new
    {
        Message = "Ordering.API - Sipariş servisi (CQRS + Saga/Outbox). Giriş yaparak sipariş oluşturabilir veya listeyi görebilirsiniz.",
        Time = DateTime.UtcNow
    }));

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<Order>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new GetOrdersQuery(), cancellationToken);
        return Ok(ResultDto<IEnumerable<Order>>.Success(list));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpGet("my")]
    public async Task<ActionResult<ResultDto<IEnumerable<Order>>>> GetMyOrders(CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? string.Empty;
        var list = await _mediator.Send(new GetMyOrdersQuery(username), cancellationToken);
        return Ok(ResultDto<IEnumerable<Order>>.Success(list));
    }

    [Authorize(Roles = "Admin,User")]
    [HttpPost]
    public async Task<ActionResult<ResultDto<Order>>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value ?? "unknown";
        try
        {
            var result = await _mediator.Send(new CreateOrderCommand(
                request.CustomerId,
                request.ProductId,
                request.Quantity,
                request.UnitPrice,
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
