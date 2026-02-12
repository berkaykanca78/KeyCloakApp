namespace GatewayApi;

/// <summary>
/// Ocelot pipeline'ından önce çalışır; Authorization header'ını AsyncLocal'a yazar.
/// DelegatingHandler'da HttpContext bazen null olduğu için handler bu değeri kullanır.
/// </summary>
public sealed class ForwardAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public ForwardAuthorizationMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Authorization", out var auth) && !string.IsNullOrEmpty(auth))
            AuthorizationHeaderHolder.Authorization = auth.ToString();
        else
            AuthorizationHeaderHolder.Authorization = null;
        return _next(context);
    }
}
