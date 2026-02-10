using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

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
    /// Bu token'ı FirstApp ve SecondApp isteklerinde Authorization: Bearer &lt;access_token&gt; olarak kullanın.
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
}

public record LoginRequest(string Username, string Password);
public record RefreshRequest(string RefreshToken);

public class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
    [JsonPropertyName("refresh_expires_in")]
    public int? RefreshExpiresIn { get; set; }
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    [JsonPropertyName("session_state")]
    public string? SessionState { get; set; }
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
