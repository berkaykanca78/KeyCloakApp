using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;
using AuthApi.Services;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IKeycloakAdminService _keycloakAdmin;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration, IKeycloakAdminService keycloakAdmin, ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _keycloakAdmin = keycloakAdmin;
        _logger = logger;
    }

    /// <summary>
    /// Keycloak ile giriş: kullanıcı adı ve şifre gönderin, access_token alın.
    /// Bu token'ı OrderApi ve InventoryApi isteklerinde Authorization: Bearer &lt;access_token&gt; olarak kullanın.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(KeycloakTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "username ve password gerekli." });

        var authority = _configuration["Keycloak:Authority"]?.TrimEnd('/') ?? "http://localhost:8080/realms/KeyCloakApp";
        var clientId = _configuration["Keycloak:ClientId"] ?? "backend-api";
        var clientSecret = _configuration["Keycloak:ClientSecret"] ?? "";

        var tokenUrl = $"{authority}/protocol/openid-connect/token";
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["username"] = request.Username,
            ["password"] = request.Password
        };

        using var client = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(form);
        var response = await client.PostAsync(tokenUrl, content, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, new { error = "Keycloak hatası", detail = json });

        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<KeycloakTokenResponse>(json);
        return Ok(tokenResponse);
    }

    /// <summary>
    /// Refresh token ile yeni access_token alın.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(KeycloakTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new { error = "refresh_token gerekli." });

        var authority = _configuration["Keycloak:Authority"]?.TrimEnd('/') ?? "http://localhost:8080/realms/KeyCloakApp";
        var clientId = _configuration["Keycloak:ClientId"] ?? "backend-api";
        var clientSecret = _configuration["Keycloak:ClientSecret"] ?? "";

        var tokenUrl = $"{authority}/protocol/openid-connect/token";
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = request.RefreshToken
        };

        using var client = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(form);
        var response = await client.PostAsync(tokenUrl, content, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, new { error = "Keycloak hatası", detail = json });

        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<KeycloakTokenResponse>(json);
        return Ok(tokenResponse);
    }

    /// <summary>
    /// Keycloak'ta kullanıcı oluşturur (user role atanır) ve OrderApi'de Customer kaydı açar.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Kullanıcı adı ve şifre gerekli." });
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "E-posta gerekli." });
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return BadRequest(new { error = "Ad ve soyad gerekli." });

        var realm = _configuration["Keycloak:Realm"] ?? "KeyCloakApp";
        var (success, userId, error) = await _keycloakAdmin.CreateUserAsync(
            realm, request.Username, request.Email, request.Password,
            request.FirstName, request.LastName, cancellationToken);
        if (!success || string.IsNullOrEmpty(userId))
            return BadRequest(new { error = error ?? "Keycloak kullanıcı oluşturulamadı." });

        // Users => Role Mapping: realm role "User" atanır (Keycloak UI ile aynı isim)
        await _keycloakAdmin.AssignRealmRoleAsync(realm, userId, "User", cancellationToken);

        var orderApiBase = _configuration["OrderApi:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:5001";
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var customerPayload = new
            {
                keycloakSub = userId,
                firstName = request.FirstName,
                lastName = request.LastName,
                address = request.Address,
                cityId = request.CityId,
                districtId = request.DistrictId,
                cardLast4 = request.CardLast4
            };
            var content = new StringContent(JsonSerializer.Serialize(customerPayload), System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{orderApiBase}/customers", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Keycloak user created but OrderApi Customer failed: {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "OrderApi Customer oluşturulamadı");
        }

        return Ok(new { message = "Kayıt başarılı. Giriş yapabilirsiniz.", userId });
    }

    /// <summary>
    /// Giriş yapmış kullanıcının bilgilerini döner. Authorization: Bearer &lt;access_token&gt; gerekir.
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Profile()
    {
        var username = User.Identity?.Name ?? User.FindFirst("preferred_username")?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var sub = User.FindFirst("sub")?.Value;
        return Ok(new
        {
            username,
            sub,
            roles,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
}
