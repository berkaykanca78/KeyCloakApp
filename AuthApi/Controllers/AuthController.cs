using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Models;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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
