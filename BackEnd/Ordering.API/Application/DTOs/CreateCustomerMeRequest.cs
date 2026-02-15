namespace Ordering.API.Application.DTOs;

/// <summary>
/// Giriş yapmış kullanıcının kendi müşteri kaydını oluşturması/güncellemesi (JWT sub kullanılır).
/// Tüm alanlar opsiyonel; boşsa JWT claim'lerden (given_name, family_name) doldurulur.
/// </summary>
public record CreateCustomerMeRequest(
    string? FirstName = null,
    string? LastName = null,
    string? Address = null,
    int? CityId = null,
    int? DistrictId = null,
    string? CardLast4 = null);
