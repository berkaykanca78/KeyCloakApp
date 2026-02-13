using System.Text.Json;
using Shared.Api;

namespace OrderApi.Infrastructure.Services;

/// <summary>
/// HTTP ile InventoryApi /inventory/availability çağrısı.
/// </summary>
public class InventoryAvailabilityClient : IInventoryAvailabilityClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _httpClient;

    public InventoryAvailabilityClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool IsAvailable, string? Message, int AvailableQuantity)> CheckAsync(string productName, int quantity, CancellationToken cancellationToken = default)
    {
        var url = $"inventory/availability?productName={Uri.EscapeDataString(productName)}&quantity={quantity}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        ResultDto<InventoryAvailabilityResponse>? result;
        try
        {
            result = JsonSerializer.Deserialize<ResultDto<InventoryAvailabilityResponse>>(json, JsonOptions);
        }
        catch
        {
            return (false, "Stok servisi yanıtı okunamadı.", 0);
        }

        var data = result?.Data;
        if (data == null && !response.IsSuccessStatusCode)
            return (false, result?.Message ?? "Stok kontrolü yapılamadı.", 0);
        if (data == null)
            return (false, result?.Message ?? "Geçersiz yanıt.", 0);
        return (data.IsAvailable, data.Message, data.AvailableQuantity);
    }
}
