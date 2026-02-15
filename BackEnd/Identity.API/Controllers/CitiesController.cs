using Microsoft.AspNetCore.Mvc;
using Identity.API.Models;
using Identity.API.Services;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController : ControllerBase
{
    private readonly ICityRedisService _cityService;

    public CitiesController(ICityRedisService cityService) => _cityService = cityService;

    /// <summary>Türkiye illeri (Redis'ten; plaka sırasına göre).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCities(CancellationToken cancellationToken)
    {
        var list = await _cityService.GetCitiesAsync(cancellationToken);
        if (list.Count == 0)
            return NotFound("İl listesi henüz yüklenmedi. Redis seed çalıştırılıyor olabilir.");
        return Ok(list);
    }

    /// <summary>Belirtilen ile ait ilçeler (cityId = plaka kodu, örn. 1 = Adana).</summary>
    [HttpGet("{cityId:int}/districts")]
    public async Task<ActionResult<IReadOnlyList<DistrictDto>>> GetDistricts(int cityId, CancellationToken cancellationToken)
    {
        var list = await _cityService.GetDistrictsAsync(cityId, cancellationToken);
        return Ok(list);
    }
}
