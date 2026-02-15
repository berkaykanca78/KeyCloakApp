using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Basket.API.Application.Commands;
using Basket.API.Application.DTOs;
using Basket.API.Domain.Aggregates;

namespace Basket.API.Application.Ports;

/// <summary>
/// Inbound port: Sepet uygulama servisi. Controller bu port üzerinden işlem yapar.
/// </summary>
public interface IBasketService
{
    /// <summary>Sepet kimliği: query buyerId &gt; JWT sub &gt; X-Basket-Id header &gt; yeni Guid. Yeni Guid üretilirse setResponseHeader ile response'a eklenir.</summary>
    string ResolveBuyerId(string? queryBuyerId, ClaimsPrincipal user, IHeaderDictionary requestHeaders, Action<string, string> setResponseHeader);
    CustomerBasketDto MapToDto(CustomerBasket basket);

    /// <summary>Sepeti DTO olarak getirir; sepet yoksa boş DTO döner.</summary>
    Task<CustomerBasketDto> GetBasketDtoAsync(string buyerId, CancellationToken cancellationToken = default);
    Task<CustomerBasket?> GetBasketAsync(string buyerId, CancellationToken cancellationToken = default);
    Task<CustomerBasketCommandResult> AddItemAsync(string buyerId, string productId, string productName, int quantity, string? inventoryItemId = null, decimal? unitPrice = null, string? currency = null, CancellationToken cancellationToken = default);
    Task<CustomerBasketCommandResult> UpdateItemAsync(string buyerId, string productId, int quantity, CancellationToken cancellationToken = default);
    Task<CustomerBasketCommandResult> RemoveItemAsync(string buyerId, string productId, CancellationToken cancellationToken = default);
    Task ClearAsync(string buyerId, CancellationToken cancellationToken = default);
}
