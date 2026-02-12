using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;
using InventoryApi.Data;

namespace InventoryApi.Consumers;

/// <summary>
/// Sipariş verildiğinde OrderApi'den gelen OrderPlaced event'ini dinler;
/// ilgili ürünü ProductName ile bulup stoktan miktar düşer.
/// </summary>
public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly InventoryDbContext _db;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(InventoryDbContext db, ILogger<OrderPlacedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg = context.Message;
        var item = await _db.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductName == msg.ProductName, context.CancellationToken);

        if (item == null)
        {
            _logger.LogWarning("OrderPlaced: Ürün bulunamadı, stok düşülmedi. ProductName={ProductName}, OrderId={OrderId}",
                msg.ProductName, msg.OrderId);
            return;
        }

        if (item.Quantity < msg.Quantity)
        {
            _logger.LogWarning(
                "OrderPlaced: Yetersiz stok. ProductName={ProductName}, OrderId={OrderId}, İstenen={Quantity}, Mevcut={Available}",
                msg.ProductName, msg.OrderId, msg.Quantity, item.Quantity);
            item.Quantity = 0;
        }
        else
        {
            item.Quantity -= msg.Quantity;
        }

        await _db.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation(
            "OrderPlaced: Stok güncellendi. ProductName={ProductName}, OrderId={OrderId}, Düşülen={Quantity}, YeniStok={NewQuantity}",
            msg.ProductName, msg.OrderId, msg.Quantity, item.Quantity);
    }
}
