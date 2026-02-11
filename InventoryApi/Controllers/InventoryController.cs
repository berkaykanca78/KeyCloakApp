using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Entities;
using InventoryApi.Models;

namespace InventoryApi.Controllers;

/// <summary>
/// Envanter / Stok API'si. Ortak Keycloak token ile erişilir. Veri PostgreSQL Inventory veritabanında.
/// </summary>
[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public InventoryController(InventoryDbContext db)
    {
        _db = db;
    }

    /// <summary>Token gerekmez; ürün listesi (sadece okuma).</summary>
    [HttpGet("public")]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var list = await _db.InventoryItems
            .Select(i => new { i.Id, i.ProductName, InStock = i.Quantity > 0 })
            .ToListAsync(cancellationToken);
        return Ok(new { Message = "InventoryApi - Stok servisi. Giriş yaparak detay ve güncelleme yapabilirsiniz.", Items = list, Time = DateTime.UtcNow });
    }

    /// <summary>Tüm stok detayı. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _db.InventoryItems.ToListAsync(cancellationToken));

    /// <summary>Tek ürün stok bilgisi. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _db.InventoryItems.FindAsync([id], cancellationToken);
        if (item == null) return NotFound();
        return Ok(item);
    }

    /// <summary>Stok miktarı güncelle. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateQuantity(int id, [FromBody] UpdateQuantityRequest request, CancellationToken cancellationToken)
    {
        var item = await _db.InventoryItems.FindAsync([id], cancellationToken);
        if (item == null) return NotFound();
        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(item);
    }
}
