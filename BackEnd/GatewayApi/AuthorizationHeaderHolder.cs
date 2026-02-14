namespace GatewayApi;

/// <summary>
/// Request scope'unda Authorization header değerini taşır.
/// DelegatingHandler'da HttpContext bazen null olduğu için (Ocelot #1252) AsyncLocal kullanıyoruz.
/// </summary>
public static class AuthorizationHeaderHolder
{
    private static readonly AsyncLocal<string?> Value = new();

    public static string? Authorization
    {
        get => Value.Value;
        set => Value.Value = value;
    }
}
