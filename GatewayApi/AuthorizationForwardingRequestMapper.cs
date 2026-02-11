using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.Request.Mapper;

namespace GatewayApi;

/// <summary>
/// Ocelot IRequestMapper decorator that explicitly forwards the upstream Authorization
/// header to the downstream request. The default mapper uses TryAddWithoutValidation,
/// which can fail for the Authorization header on HttpRequestMessage.Headers.
/// </summary>
public sealed class AuthorizationForwardingRequestMapper : IRequestMapper
{
    private readonly IRequestMapper _inner = new RequestMapper();

    public HttpRequestMessage Map(HttpRequest request, DownstreamRoute downstreamRoute)
    {
        var requestMessage = _inner.Map(request, downstreamRoute);

        if (request.Headers.TryGetValue("Authorization", out var auth) && !string.IsNullOrEmpty(auth))
            requestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse(auth!);

        return requestMessage;
    }
}
