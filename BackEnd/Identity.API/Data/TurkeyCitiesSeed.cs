using System.Globalization;
using System.Text.Json;
using Identity.API.Models;

namespace Identity.API.Data;

/// <summary>
/// Türkiye il/ilçe verisi — [volkansenturk/turkiye-iller-ilceler](https://github.com/volkansenturk/turkiye-iller-ilceler)
/// PHPMyAdmin export formatı: root array içinde type="table", name="il" veya "ilce", "data" dizisi.
/// </summary>
public static class TurkeyCitiesSeed
{
    private const int MinPlate = 1;
    private const int MaxPlate = 81;

    /// <summary>
    /// PHPMyAdmin il.json formatını parse eder. Sadece plaka 1–81 (Kıbrıs 501,502,503 hariç).
    /// </summary>
    public static IReadOnlyList<CityDto> ParseIlJson(string json)
    {
        var list = new List<CityDto>();
        using var doc = JsonDocument.Parse(json);
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("data", out var data))
                continue;
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("id", out var idEl) || !item.TryGetProperty("name", out var nameEl))
                    continue;
                var idStr = idEl.GetString();
                if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
                    continue;
                if (id < MinPlate || id > MaxPlate)
                    continue;
                var name = nameEl.GetString() ?? "";
                list.Add(new CityDto
                {
                    Id = id,
                    Name = ToTitleCase(name),
                    PlateCode = id
                });
            }
            break;
        }
        return list.OrderBy(c => c.Id).ToList();
    }

    /// <summary>
    /// PHPMyAdmin ilce.json formatını parse eder. il_id 1–81 olan ilçeler; id/il_id/name alanları.
    /// </summary>
    public static IReadOnlyList<DistrictDto> ParseIlceJson(string json)
    {
        var list = new List<DistrictDto>();
        using var doc = JsonDocument.Parse(json);
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("data", out var data))
                continue;
            foreach (var item in data.EnumerateArray())
            {
                if (!item.TryGetProperty("id", out var idEl) ||
                    !item.TryGetProperty("il_id", out var ilIdEl) ||
                    !item.TryGetProperty("name", out var nameEl))
                    continue;
                var idStr = idEl.GetString();
                var ilIdStr = ilIdEl.GetString();
                if (string.IsNullOrEmpty(idStr) || string.IsNullOrEmpty(ilIdStr))
                    continue;
                if (!int.TryParse(ilIdStr, NumberStyles.None, CultureInfo.InvariantCulture, out var cityId))
                    continue;
                if (cityId < MinPlate || cityId > MaxPlate)
                    continue;
                if (!int.TryParse(idStr, NumberStyles.None, CultureInfo.InvariantCulture, out var districtId))
                    continue;
                var name = nameEl.GetString() ?? "";
                list.Add(new DistrictDto
                {
                    Id = districtId,
                    CityId = cityId,
                    Name = ToTitleCase(name)
                });
            }
            break;
        }
        return list;
    }

    /// <summary>
    /// İlçe listesini il id'ye göre gruplar (Redis'e districts:1, districts:2 ... yazmak için).
    /// </summary>
    public static IReadOnlyDictionary<int, IReadOnlyList<DistrictDto>> GroupDistrictsByCityId(IReadOnlyList<DistrictDto> districts)
    {
        return districts
            .GroupBy(d => d.CityId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<DistrictDto>)g.ToList());
    }

    private static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var ti = CultureInfo.GetCultureInfo("tr-TR").TextInfo;
        return ti.ToTitleCase(value.Trim().ToLower(CultureInfo.GetCultureInfo("tr-TR")));
    }

    /// <summary>
    /// JSON yoksa kullanılacak gömülü 81 il (plaka sırasına göre).
    /// </summary>
    public static IReadOnlyList<CityDto> GetEmbeddedCities()
    {
        var cities = new (int Id, string Name)[]
        {
            (1, "Adana"), (2, "Adıyaman"), (3, "Afyonkarahisar"), (4, "Ağrı"), (5, "Amasya"), (6, "Ankara"), (7, "Antalya"),
            (8, "Artvin"), (9, "Aydın"), (10, "Balıkesir"), (11, "Bilecik"), (12, "Bingöl"), (13, "Bitlis"), (14, "Bolu"),
            (15, "Burdur"), (16, "Bursa"), (17, "Çanakkale"), (18, "Çankırı"), (19, "Çorum"), (20, "Denizli"),
            (21, "Diyarbakır"), (22, "Edirne"), (23, "Elazığ"), (24, "Erzincan"), (25, "Erzurum"), (26, "Eskişehir"),
            (27, "Gaziantep"), (28, "Giresun"), (29, "Gümüşhane"), (30, "Hakkari"), (31, "Hatay"), (32, "Isparta"),
            (33, "Mersin"), (34, "İstanbul"), (35, "İzmir"), (36, "Kars"), (37, "Kastamonu"), (38, "Kayseri"),
            (39, "Kırklareli"), (40, "Kırşehir"), (41, "Kocaeli"), (42, "Konya"), (43, "Kütahya"), (44, "Malatya"),
            (45, "Manisa"), (46, "Kahramanmaraş"), (47, "Mardin"), (48, "Muğla"), (49, "Muş"), (50, "Nevşehir"),
            (51, "Niğde"), (52, "Ordu"), (53, "Rize"), (54, "Sakarya"), (55, "Samsun"), (56, "Siirt"), (57, "Sinop"),
            (58, "Sivas"), (59, "Tekirdağ"), (60, "Tokat"), (61, "Trabzon"), (62, "Tunceli"), (63, "Şanlıurfa"),
            (64, "Uşak"), (65, "Van"), (66, "Yozgat"), (67, "Zonguldak"), (68, "Aksaray"), (69, "Bayburt"),
            (70, "Karaman"), (71, "Kırıkkale"), (72, "Batman"), (73, "Şırnak"), (74, "Bartın"), (75, "Ardahan"),
            (76, "Iğdır"), (77, "Yalova"), (78, "Karabük"), (79, "Kilis"), (80, "Osmaniye"), (81, "Düzce")
        };
        return cities.Select(c => new CityDto { Id = c.Id, Name = c.Name, PlateCode = c.Id }).ToList();
    }

    /// <summary>
    /// Gömülü ilçe yok; JSON veya harici dosya kullanılmalı. İl id için boş liste döner (fallback).
    /// </summary>
    public static IReadOnlyList<DistrictDto> GetEmbeddedDistricts(int cityId)
    {
        if (cityId < MinPlate || cityId > MaxPlate)
            return Array.Empty<DistrictDto>();
        return new List<DistrictDto> { new DistrictDto { Id = cityId * 100 + 1, Name = "Merkez", CityId = cityId } };
    }
}
