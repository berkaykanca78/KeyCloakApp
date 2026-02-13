using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApi.Application.DTOs;
using InventoryApi.Application.UseCases;
using InventoryApi.Domain.Aggregates;

namespace InventoryApi.Presentation.Controllers;

/// <summary>
/// Stok API'si. DDD Presentation: Controller sadece use case'leri çağırır.
/// </summary>
[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly GetInventoryPublicUseCase _getPublicUseCase;
    private readonly GetAllInventoryUseCase _getAllUseCase;
    private readonly GetInventoryByIdUseCase _getByIdUseCase;
    private readonly UpdateQuantityUseCase _updateQuantityUseCase;

    public InventoryController(
        GetInventoryPublicUseCase getPublicUseCase,
        GetAllInventoryUseCase getAllUseCase,
        GetInventoryByIdUseCase getByIdUseCase,
        UpdateQuantityUseCase updateQuantityUseCase)
    {
        _getPublicUseCase = getPublicUseCase;
        _getAllUseCase = getAllUseCase;
        _getByIdUseCase = getByIdUseCase;
        _updateQuantityUseCase = updateQuantityUseCase;
    }

    /// <summary>Token gerekmez; ürün listesi (sadece okuma).</summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var (message, items, time) = await _getPublicUseCase.ExecuteAsync(cancellationToken);
        return Ok(new { Message = message, Items = items, Time = time });
    }

    /// <summary>Tüm stok detayı. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _getAllUseCase.ExecuteAsync(cancellationToken);
        return Ok(list);
    }

    /// <summary>Tek ürün stok bilgisi. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        if (item == null) return NotFound();
        return Ok(item);
    }

    /// <summary>Stok miktarı güncelle. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateQuantityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _updateQuantityUseCase.ExecuteAsync(id, request.Quantity, cancellationToken);
            if (item == null) return NotFound();
            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
