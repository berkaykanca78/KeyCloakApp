namespace OrderApi.Domain.Aggregates;

/// <summary>
/// Müşteri — Keycloak sub ile eşlenir; adres ve kart bilgisi (kayıt sırasında alınır).
/// </summary>
public class Customer
{
    public Guid Id { get; private set; }
    public string KeycloakSub { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public int? CityId { get; private set; }
    public int? DistrictId { get; private set; }
    public string? CardLast4 { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Customer() { }

    public static Customer Create(string keycloakSub, string firstName, string lastName, string? address = null, int? cityId = null, int? districtId = null, string? cardLast4 = null)
    {
        if (string.IsNullOrWhiteSpace(keycloakSub)) throw new ArgumentException("Keycloak sub gerekli.", nameof(keycloakSub));
        return new Customer
        {
            Id = Guid.NewGuid(),
            KeycloakSub = keycloakSub.Trim(),
            FirstName = (firstName ?? "").Trim(),
            LastName = (lastName ?? "").Trim(),
            Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
            CityId = cityId,
            DistrictId = districtId,
            CardLast4 = string.IsNullOrWhiteSpace(cardLast4) ? null : cardLast4.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
