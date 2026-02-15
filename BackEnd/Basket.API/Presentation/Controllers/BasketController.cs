using Microsoft.AspNetCore.Mvc;
using Basket.API.Application.DTOs;
using Basket.API.Application.Ports;

namespace Basket.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketController(IBasketService basketService) => _basketService = basketService;

    private string ResolveBuyerId(string? buyerId) =>
        _basketService.ResolveBuyerId(buyerId, User, Request.Headers, (name, value) => Response.Headers.Append(name, value));

    /// <summary>Sepeti getirir. buyerId: query ?buyerId=... veya X-Basket-Id header (anonim); giriş yapılıysa JWT sub kullanılır.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? buyerId, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = ResolveBuyerId(buyerId);
        var dto = await _basketService.GetBasketDtoAsync(resolvedBuyerId, cancellationToken);
        return Ok(dto);
    }

    /// <summary>Sepete ürün ekler veya miktarını günceller. buyerId: query veya X-Basket-Id.</summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromQuery] string? buyerId, [FromBody] AddBasketItemRequest request, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = ResolveBuyerId(buyerId);
        var result = await _basketService.AddItemAsync(
            resolvedBuyerId,
            request.ProductId,
            request.ProductName ?? request.ProductId,
            request.Quantity < 1 ? 1 : request.Quantity,
            request.InventoryItemId,
            request.UnitPrice,
            request.Currency,
            cancellationToken);
        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });
        return Ok(_basketService.MapToDto(result.Basket!));
    }

    /// <summary>Sepetteki bir ürünün miktarını günceller. 0 ise satır silinir. buyerId: query veya X-Basket-Id.</summary>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(string productId, [FromQuery] string? buyerId, [FromBody] UpdateBasketItemRequest request, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = ResolveBuyerId(buyerId);
        var result = await _basketService.UpdateItemAsync(resolvedBuyerId, productId, request.Quantity, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.ErrorMessage });
        return Ok(_basketService.MapToDto(result.Basket!));
    }

    /// <summary>Sepetten ürün kaldırır. buyerId: query veya X-Basket-Id.</summary>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem(string productId, [FromQuery] string? buyerId, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = ResolveBuyerId(buyerId);
        var result = await _basketService.RemoveItemAsync(resolvedBuyerId, productId, cancellationToken);
        return Ok(_basketService.MapToDto(result.Basket!));
    }

    /// <summary>Sepeti tamamen temizler. buyerId: query veya X-Basket-Id.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear([FromQuery] string? buyerId, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = ResolveBuyerId(buyerId);
        await _basketService.ClearAsync(resolvedBuyerId, cancellationToken);
        return NoContent();
    }
}
