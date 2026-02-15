namespace AuthApi.Models;

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? CityId { get; set; }
    public int? DistrictId { get; set; }
    public string? CardLast4 { get; set; }
}
