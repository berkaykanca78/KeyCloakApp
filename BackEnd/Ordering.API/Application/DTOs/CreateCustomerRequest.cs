namespace Ordering.API.Application.DTOs;

public record CreateCustomerRequest(
    string KeycloakSub,
    string FirstName,
    string LastName,
    string? Address = null,
    int? CityId = null,
    int? DistrictId = null,
    string? CardLast4 = null);
