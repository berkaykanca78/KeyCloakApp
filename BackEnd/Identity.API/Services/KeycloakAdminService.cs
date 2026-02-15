using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Identity.API.Services;

public interface IKeycloakAdminService
{
    Task<(bool Success, string? UserId, string? Error)> CreateUserAsync(string realm, string username, string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<bool> AssignRealmRoleAsync(string realm, string userId, string roleName, CancellationToken cancellationToken = default);
}

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private async Task<string?> GetAdminTokenAsync(CancellationToken cancellationToken)
    {
        var adminUrl = _configuration["Keycloak:AdminUrl"]?.TrimEnd('/') ?? "http://localhost:8080";
        var adminUser = _configuration["Keycloak:AdminUsername"] ?? "admin";
        var adminPassword = _configuration["Keycloak:AdminPassword"] ?? "admin";

        var tokenUrl = $"{adminUrl}/realms/master/protocol/openid-connect/token";
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = adminUser,
            ["password"] = adminPassword
        };

        using var client = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(form);
        var response = await client.PostAsync(tokenUrl, content, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("access_token", out var tok) ? tok.GetString() : null;
    }

    public async Task<(bool Success, string? UserId, string? Error)> CreateUserAsync(string realm, string username, string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var adminUrl = _configuration["Keycloak:AdminUrl"]?.TrimEnd('/') ?? "http://localhost:8080";
        var token = await GetAdminTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
            return (false, null, "Keycloak admin token alınamadı. Admin kullanıcı/şifre kontrol edin.");

        var url = $"{adminUrl}/admin/realms/{realm}/users";
        var body = new
        {
            username,
            email,
            firstName,
            lastName,
            enabled = true,
            credentials = new[] { new { type = "password", value = password, temporary = false } }
        };

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Created && response.Headers.Location != null)
        {
            var path = response.Headers.Location.ToString();
            var userId = path.TrimEnd('/').Split('/').LastOrDefault();
            return (true, userId, null);
        }
        var err = await response.Content.ReadAsStringAsync(cancellationToken);
        return (false, null, err);
    }

    public async Task<bool> AssignRealmRoleAsync(string realm, string userId, string roleName, CancellationToken cancellationToken = default)
    {
        var adminUrl = _configuration["Keycloak:AdminUrl"]?.TrimEnd('/') ?? "http://localhost:8080";
        var token = await GetAdminTokenAsync(cancellationToken);
        if (string.IsNullOrEmpty(token))
            return false;

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var rolesUrl = $"{adminUrl}/admin/realms/{realm}/roles";
        var rolesResponse = await client.GetAsync(rolesUrl, cancellationToken);
        if (!rolesResponse.IsSuccessStatusCode)
            return false;
        var rolesJson = await rolesResponse.Content.ReadAsStringAsync(cancellationToken);
        using var rolesDoc = JsonDocument.Parse(rolesJson);
        var roleId = (string?)null;
        foreach (var r in rolesDoc.RootElement.EnumerateArray())
        {
            if (r.TryGetProperty("name", out var n) && n.GetString() == roleName)
            {
                if (r.TryGetProperty("id", out var id))
                    roleId = id.GetString();
                break;
            }
        }
        if (string.IsNullOrEmpty(roleId))
            return false;

        var assignUrl = $"{adminUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm";
        var body = JsonSerializer.Serialize(new[] { new { id = roleId, name = roleName } });
        var assignContent = new StringContent(body, Encoding.UTF8, "application/json");
        var assignResponse = await client.PostAsync(assignUrl, assignContent, cancellationToken);
        return assignResponse.IsSuccessStatusCode;
    }
}
