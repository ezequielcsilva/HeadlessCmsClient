using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using System.Net.Http.Headers;

namespace HeadlessCmsClient.Infrastructure.Http;

public class AuthHandler(ITokenProvider tokenProvider, AuthRequest authRequest) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResponse = await tokenProvider.GetTokenAsync(authRequest, cancellationToken);
        if (tokenResponse is null || tokenProvider.IsTokenExpired())
        {
            tokenResponse = await tokenProvider.RefreshTokenAsync(cancellationToken);
        }

        if(tokenResponse is null)
        {
            throw new InvalidOperationException("No token available.");
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse?.BearerToken);
        return await base.SendAsync(request, cancellationToken);
    }
}