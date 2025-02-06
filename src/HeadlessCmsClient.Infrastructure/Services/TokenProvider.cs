using HeadlessCmsClient.Core.Constants;
using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HeadlessCmsClient.Infrastructure.Services;

internal sealed class TokenProvider : ITokenProvider
{
    private readonly HttpClient _httpClient;
    private TokenResponse? _tokenResponse;

    public TokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<TokenResponse?> GetTokenAsync(AuthRequest request, CancellationToken cancellationToken)
    {
        if (_tokenResponse is not null && !IsTokenExpired())
            return _tokenResponse;

        var requestBody = JsonSerializer.Serialize(request);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiRoutes.Auth, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync(cancellationToken);
        _tokenResponse = JsonSerializer.Deserialize<TokenResponse>(data);
        return _tokenResponse;
    }

    public async Task<TokenResponse?> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        if (_tokenResponse == null)
            throw new InvalidOperationException("No token to refresh.");

        var request = new HttpRequestMessage(HttpMethod.Get, ApiRoutes.RefreshAuth);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenResponse.BearerToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);

        return _tokenResponse;
    }

    public bool IsTokenExpired()
    {
        return string.IsNullOrEmpty(_tokenResponse?.BearerToken) || _tokenResponse.ExpiryDate <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}