using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApi.Controllers;

/// <summary>
/// Envanter / Stok API'si. Ortak Keycloak token ile erişilir.
/// </summary>
[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private static readonly List<InventoryItem> _items = new()
    {
        new InventoryItem(1, "Ürün A", 100, "Depo-1"),
        new InventoryItem(2, "Ürün B", 50, "Depo-1"),
        new InventoryItem(3, "Ürün C", 200, "Depo-2"),
    };

    /// <summary>Token gerekmez; ürün listesi (sadece okuma).</summary>
    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        var list = _items.Select(i => new { i.Id, i.ProductName, InStock = i.Quantity > 0 });
        return Ok(new { Message = "InventoryApi - Stok servisi. Giriş yaparak detay ve güncelleme yapabilirsiniz.", Items = list, Time = DateTime.UtcNow });
    }

    /// <summary>Tüm stok detayı. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult<IEnumerable<InventoryItem>> GetAll() => Ok(_items);

    /// <summary>Tek ürün stok bilgisi. Admin veya User.</summary>
    [Authorize(Roles = "Admin,User")]
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var item = _items.Find(i => i.Id == id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    /// <summary>Stok miktarı güncelle. Sadece Admin.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public IActionResult UpdateQuantity(int id, [FromBody] UpdateQuantityRequest request)
    {
        var item = _items.Find(i => i.Id == id);
        if (item == null) return NotFound();
        var updated = item with { Quantity = request.Quantity };
        var index = _items.IndexOf(item);
        _items[index] = updated;
        return Ok(updated);
    }
}

public record InventoryItem(int Id, string ProductName, int Quantity, string Location);
public record UpdateQuantityRequest(int Quantity);
