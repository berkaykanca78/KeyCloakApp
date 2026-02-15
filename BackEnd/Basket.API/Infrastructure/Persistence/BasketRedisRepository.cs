using System.Text.Json;
using Basket.API.Domain.Aggregates;
using Basket.API.Domain.Repositories;
using Basket.API.Domain.ValueObjects;
using StackExchange.Redis;

namespace Basket.API.Infrastructure.Persistence;

/// <summary>
/// Redis tabanlı sepet repository. DDD aggregate'ı Redis'te JSON olarak saklar.
/// </summary>
public class BasketRedisRepository : IBasketRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    private const string KeyPrefix = "basket:";
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(30);

    private readonly IConnectionMultiplexer _redis;

    public BasketRedisRepository(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<CustomerBasket?> GetByBuyerIdAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = KeyPrefix + buyerId;
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return null;
        var dto = JsonSerializer.Deserialize<RedisBasketDto>(value.ToString(), JsonOptions);
        if (dto == null)
            return null;
        var basket = CustomerBasket.Create(dto.BuyerId);
        foreach (var i in dto.Items ?? [])
            basket.AddItem(i.ProductId, i.ProductName ?? i.ProductId, i.Quantity, i.InventoryItemId, i.UnitPrice, i.Currency);
        return basket;
    }

    public async Task SaveAsync(CustomerBasket basket, CancellationToken cancellationToken = default)
    {
        var dto = new RedisBasketDto
        {
            BuyerId = basket.BuyerId,
            Items = basket.Items.Select(i => new RedisBasketItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                InventoryItemId = i.InventoryItemId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Currency = i.Currency
            }).ToList()
        };
        var db = _redis.GetDatabase();
        var key = KeyPrefix + basket.BuyerId;
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        await db.StringSetAsync(key, json, Ttl);
    }

    public async Task<bool> DeleteAsync(string buyerId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = KeyPrefix + buyerId;
        return await db.KeyDeleteAsync(key);
    }

    private class RedisBasketDto
    {
        public string BuyerId { get; set; } = string.Empty;
        public List<RedisBasketItemDto>? Items { get; set; }
    }

    private class RedisBasketItemDto
    {
        public string ProductId { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public string? InventoryItemId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Currency { get; set; }
    }
}
