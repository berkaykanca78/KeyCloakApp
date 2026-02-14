using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApi.Application.DTOs;
using InventoryApi.Application.UseCases;
using InventoryApi.Domain.Aggregates;
using Shared.Api;

namespace InventoryApi.Presentation.Controllers;

/// <summary>
/// Stok API'si. Tüm yanıtlar ResultDto ile sarılı.
/// </summary>
[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly GetInventoryPublicUseCase _getPublicUseCase;
    private readonly GetAllInventoryUseCase _getAllUseCase;
    private readonly GetInventoryByIdUseCase _getByIdUseCase;
    private readonly UpdateQuantityUseCase _updateQuantityUseCase;
    private readonly CheckAvailabilityUseCase _checkAvailabilityUseCase;

    public InventoryController(
        GetInventoryPublicUseCase getPublicUseCase,
        GetAllInventoryUseCase getAllUseCase,
        GetInventoryByIdUseCase getByIdUseCase,
        UpdateQuantityUseCase updateQuantityUseCase,
        CheckAvailabilityUseCase checkAvailabilityUseCase)
    {
        _getPublicUseCase = getPublicUseCase;
        _getAllUseCase = getAllUseCase;
        _getByIdUseCase = getByIdUseCase;
        _updateQuantityUseCase = updateQuantityUseCase;
        _checkAvailabilityUseCase = checkAvailabilityUseCase;
    }

    /// <summary>Token gerekmez; ürün listesi (sadece okuma).</summary>
    [HttpGet("public")]
    public async Task<ActionResult<ResultDto<object>>> GetPublic(CancellationToken cancellationToken)
    {
        var (message, items, time) = await _getPublicUseCase.ExecuteAsync(cancellationToken);
        return Ok(ResultDto<object>.Success(new { Message = message, Items = items, Time = time }));
    }

    /// <summary>Tüm stok detayı. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<ResultDto<IEnumerable<InventoryItem>>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _getAllUseCase.ExecuteAsync(cancellationToken);
        return Ok(ResultDto<IEnumerable<InventoryItem>>.Success(list));
    }

    /// <summary>Tek ürün stok bilgisi. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResultDto<InventoryItem>>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _getByIdUseCase.ExecuteAsync(id, cancellationToken);
        if (item == null)
            return NotFound(ResultDto<InventoryItem>.Failure("Ürün bulunamadı."));
        return Ok(ResultDto<InventoryItem>.Success(item));
    }

    /// <summary>Stok yeterliliği kontrolü (OrderApi sipariş öncesi çağırır). Token gerekmez; servisler arası.</summary>
    [HttpGet("availability")]
    public async Task<ActionResult<ResultDto<AvailabilityResponse>>> CheckAvailability(
        [FromQuery] string productName,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        var result = await _checkAvailabilityUseCase.ExecuteAsync(productName, quantity, cancellationToken);
        var response = new AvailabilityResponse(
            result.IsAvailable,
            result.ProductName,
            result.AvailableQuantity,
            result.Message);
        if (result.IsAvailable)
            return Ok(ResultDto<AvailabilityResponse>.Success(response, result.Message));
        return BadRequest(ResultDto<AvailabilityResponse>.Failure(result.Message, new[] { $"{result.ProductName}: Mevcut stok {result.AvailableQuantity}, istenen {result.RequestedQuantity}." }));
    }

    /// <summary>Stok miktarı güncelle. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ResultDto<InventoryItem>>> UpdateQuantity(int id, [FromBody] UpdateQuantityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _updateQuantityUseCase.ExecuteAsync(id, request.Quantity, cancellationToken);
            if (item == null)
                return NotFound(ResultDto<InventoryItem>.Failure("Ürün bulunamadı."));
            return Ok(ResultDto<InventoryItem>.Success(item, "Stok güncellendi."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ResultDto<InventoryItem>.Failure(ex.Message));
        }
    }
}
