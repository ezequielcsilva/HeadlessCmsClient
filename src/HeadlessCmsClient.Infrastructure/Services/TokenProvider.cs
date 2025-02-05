using HeadlessCmsClient.Core.Constants;
using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HeadlessCmsClient.Infrastructure.Services;

internal sealed class TokenProvider(HttpClient httpClient) : ITokenProvider
{
    public async Task<TokenResponse?> GetTokenAsync(AuthRequest request, CancellationToken cancellationToken)
    {
        var requestBody = JsonSerializer.Serialize(request);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiRoutes.Auth, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(data);

        return tokenResponse;
    }

    public async Task<TokenResponse?> RefreshTokenAsync(TokenResponse lastTokenResponse, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.RefreshAuth);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", lastTokenResponse.BearerToken);

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);

        return tokenResponse;
    }

    public bool IsTokenExpired(TokenResponse tokenResponse)
    {
        return string.IsNullOrEmpty(tokenResponse.BearerToken) || tokenResponse.ExpiryDate <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}