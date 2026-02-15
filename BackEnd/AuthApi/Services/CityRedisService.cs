using System.Text.Json;
using AuthApi.Data;
using AuthApi.Models;
using StackExchange.Redis;

namespace AuthApi.Services;

public class CityRedisService : ICityRedisService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private const string CitiesKey = "cities";
    private const string DistrictsKeyPrefix = "districts:";

    private const string IlJsonUrl = "https://raw.githubusercontent.com/volkansenturk/turkiye-iller-ilceler/master/il.json";
    private const string IlceJsonUrl = "https://raw.githubusercontent.com/volkansenturk/turkiye-iller-ilceler/master/ilce.json";

    private readonly IConnectionMultiplexer _redis;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpClientFactory _httpClientFactory;

    public CityRedisService(
        IConnectionMultiplexer redis,
        IWebHostEnvironment env,
        IHttpClientFactory httpClientFactory)
    {
        _redis = redis;
        _env = env;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(CitiesKey);
        if (value.IsNullOrEmpty)
            return Array.Empty<CityDto>();
        var json = value.ToString();
        var list = JsonSerializer.Deserialize<List<CityDto>>(json, JsonOptions);
        return list ?? (IReadOnlyList<CityDto>)new List<CityDto>();
    }

    public async Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(int cityId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = DistrictsKeyPrefix + cityId;
        var value = await db.StringGetAsync(key);
        if (value.IsNullOrEmpty)
            return Array.Empty<DistrictDto>();
        var json = value.ToString();
        var list = JsonSerializer.Deserialize<List<DistrictDto>>(json, JsonOptions);
        return list ?? (IReadOnlyList<DistrictDto>)new List<DistrictDto>();
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        string? ilJson = null;
        string? ilceJson = null;

        var dataPath = Path.Combine(_env.ContentRootPath, "Data");
        var ilPath = Path.Combine(dataPath, "il.json");
        var ilcePath = Path.Combine(dataPath, "ilce.json");

        if (File.Exists(ilPath))
            ilJson = await File.ReadAllTextAsync(ilPath, cancellationToken);
        if (File.Exists(ilcePath))
            ilceJson = await File.ReadAllTextAsync(ilcePath, cancellationToken);

        if (string.IsNullOrEmpty(ilJson) || string.IsNullOrEmpty(ilceJson))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "KeyCloakApp-AuthApi/1.0");
                if (string.IsNullOrEmpty(ilJson))
                    ilJson = await client.GetStringAsync(IlJsonUrl, cancellationToken);
                if (string.IsNullOrEmpty(ilceJson))
                    ilceJson = await client.GetStringAsync(IlceJsonUrl, cancellationToken);
            }
            catch
            {
                // GitHub erişilemezse gömülü veri kullanılır
            }
        }

        IReadOnlyList<CityDto> cities;
        IReadOnlyDictionary<int, IReadOnlyList<DistrictDto>>? districtsByCity = null;

        if (!string.IsNullOrEmpty(ilJson))
        {
            cities = TurkeyCitiesSeed.ParseIlJson(ilJson);
            if (cities.Count == 0)
                cities = TurkeyCitiesSeed.GetEmbeddedCities();
        }
        else
        {
            cities = TurkeyCitiesSeed.GetEmbeddedCities();
        }

        if (!string.IsNullOrEmpty(ilceJson))
        {
            var allDistricts = TurkeyCitiesSeed.ParseIlceJson(ilceJson);
            districtsByCity = TurkeyCitiesSeed.GroupDistrictsByCityId(allDistricts);
        }

        await db.StringSetAsync(CitiesKey, JsonSerializer.Serialize(cities, JsonOptions));

        foreach (var city in cities)
        {
            IReadOnlyList<DistrictDto> districts;
            if (districtsByCity != null && districtsByCity.TryGetValue(city.Id, out var list))
                districts = list;
            else
                districts = TurkeyCitiesSeed.GetEmbeddedDistricts(city.Id);
            await db.StringSetAsync(DistrictsKeyPrefix + city.Id, JsonSerializer.Serialize(districts, JsonOptions));
        }
    }
}
