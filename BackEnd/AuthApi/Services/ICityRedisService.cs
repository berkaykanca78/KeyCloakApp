using AuthApi.Models;

namespace AuthApi.Services;

public interface ICityRedisService
{
    Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(int cityId, CancellationToken cancellationToken = default);
    Task SeedAsync(CancellationToken cancellationToken = default);
}
