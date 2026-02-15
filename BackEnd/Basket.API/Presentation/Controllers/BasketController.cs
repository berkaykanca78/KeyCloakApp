using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Basket.API.Application.DTOs;
using Basket.API.Application.Ports;
using Basket.API.Domain.Aggregates;
using Basket.API.Domain.ValueObjects;

namespace Basket.API.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private const string BasketIdHeader = "X-Basket-Id";
    private readonly IBasketService _basketService;

    public BasketController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    /// <summary>Sepet kimliği: query buyerId &gt; JWT sub &gt; X-Basket-Id header &gt; yeni Guid (response'da X-Basket-Id döner).</summary>
    private string GetOrCreateBuyerId(string? explicitBuyerId = null)
    {
        if (!string.IsNullOrWhiteSpace(explicitBuyerId))
            return explicitBuyerId.Trim();
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!string.IsNullOrEmpty(sub))
            return sub;
        var header = Request.Headers[BasketIdHeader].FirstOrDefault();
        if (!string.IsNullOrEmpty(header))
            return header;
        var newId = Guid.NewGuid().ToString("N");
        Response.Headers.Append(BasketIdHeader, newId);
        return newId;
    }

    private static CustomerBasketDto MapToDto(CustomerBasket basket)
    {
        return new CustomerBasketDto
        {
            BuyerId = basket.BuyerId,
            Items = basket.Items.Select(i => new BasketItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                InventoryItemId = i.InventoryItemId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Currency = i.Currency
            }).ToList()
        };
    }

    /// <summary>Sepeti getirir. buyerId: query ?buyerId=... veya X-Basket-Id header (anonim); giriş yapılıysa JWT sub kullanılır.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? buyerId, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = GetOrCreateBuyerId(buyerId);
        var basket = await _basketService.GetBasketAsync(resolvedBuyerId, cancellationToken);
        if (basket == null)
        {
            var dto = new CustomerBasketDto { BuyerId = resolvedBuyerId, Items = new List<BasketItemDto>() };
            return Ok(dto);
        }
        return Ok(MapToDto(basket));
    }

    /// <summary>Sepete ürün ekler veya miktarını günceller. buyerId: query veya X-Basket-Id.</summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromQuery] string? buyerId, [FromBody] AddBasketItemRequest request, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = GetOrCreateBuyerId(buyerId);
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
        return Ok(MapToDto(result.Basket!));
    }

    /// <summary>Sepetteki bir ürünün miktarını günceller. 0 ise satır silinir. buyerId: query veya X-Basket-Id.</summary>
    [HttpPut("items/{productId}")]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(string productId, [FromQuery] string? buyerId, [FromBody] UpdateBasketItemRequest request, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = GetOrCreateBuyerId(buyerId);
        var result = await _basketService.UpdateItemAsync(resolvedBuyerId, productId, request.Quantity, cancellationToken);
        if (!result.Success)
            return NotFound(new { error = result.ErrorMessage });
        return Ok(MapToDto(result.Basket!));
    }

    /// <summary>Sepetten ürün kaldırır. buyerId: query veya X-Basket-Id.</summary>
    [HttpDelete("items/{productId}")]
    [ProducesResponseType(typeof(CustomerBasketDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem(string productId, [FromQuery] string? buyerId, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = GetOrCreateBuyerId(buyerId);
        var result = await _basketService.RemoveItemAsync(resolvedBuyerId, productId, cancellationToken);
        return Ok(MapToDto(result.Basket!));
    }

    /// <summary>Sepeti tamamen temizler. buyerId: query veya X-Basket-Id.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear([FromQuery] string? buyerId, CancellationToken cancellationToken)
    {
        var resolvedBuyerId = GetOrCreateBuyerId(buyerId);
        await _basketService.ClearAsync(resolvedBuyerId, cancellationToken);
        return NoContent();
    }
}
