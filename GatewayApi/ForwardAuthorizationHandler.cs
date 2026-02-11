using System.Net.Http.Headers;

namespace GatewayApi;

/// <summary>
/// Ocelot delegating handler: upstream'deki Authorization header'ını downstream HttpRequestMessage'a ekler.
/// </summary>
public sealed class ForwardAuthorizationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ForwardAuthorizationHandler> _logger;

    public ForwardAuthorizationHandler(IHttpContextAccessor httpContextAccessor, ILogger<ForwardAuthorizationHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization != null)
        {
            _logger.LogDebug("Downstream request already has Authorization");
        }
        else
        {
            var auth = GetUpstreamAuthorization();
            if (!string.IsNullOrEmpty(auth))
            {
                try
                {
                    request.Headers.Authorization = AuthenticationHeaderValue.Parse(auth);
                    _logger.LogInformation("Forwarded Authorization header to downstream {RequestUri}", request.RequestUri);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse/set Authorization header");
                }
            }
            else
            {
                _logger.LogWarning("No Authorization from upstream (AsyncLocal or HttpContext); downstream may return 401. Request: {RequestUri}", request.RequestUri);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string? GetUpstreamAuthorization()
    {
        if (!string.IsNullOrEmpty(AuthorizationHeaderHolder.Authorization))
            return AuthorizationHeaderHolder.Authorization;
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx?.Request?.Headers.TryGetValue("Authorization", out var value) == true && !string.IsNullOrEmpty(value))
            return value.ToString();
        return null;
    }
}
