using HeadlessCmsClient.Core.Constants;
using HeadlessCmsClient.Core.Models;
using HeadlessCmsClient.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HeadlessCmsClient.Tests;

public class TokenProviderTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly TokenProvider _tokenProvider;
    private readonly AuthRequest _authRequest;

    public TokenProviderTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        _tokenProvider = new TokenProvider(httpClient);
        _authRequest = new AuthRequest { Username = "test", Password = "password" };
    }

    private void SetupHttpResponse(HttpMethod method, string url, HttpStatusCode statusCode, object responseBody)
    {
        var json = JsonSerializer.Serialize(responseBody);
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == method &&
                    req.RequestUri!.ToString().EndsWith(url)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_WhenRequestIsSuccessful()
    {
        // Arrange
        var tokenResponse = new TokenResponse { BearerToken = "valid-token", ExpiryDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 };
        SetupHttpResponse(HttpMethod.Post, ApiRoutes.Auth, HttpStatusCode.OK, tokenResponse);

        // Act
        var result = await _tokenProvider.GetTokenAsync(_authRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("valid-token", result.BearerToken);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnCachedToken_WhenTokenIsValid()
    {
        // Arrange
        var tokenResponse = new TokenResponse { BearerToken = "cached-token", ExpiryDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 };

        SetupHttpResponse(HttpMethod.Post, ApiRoutes.Auth, HttpStatusCode.OK, tokenResponse);

        _ = await _tokenProvider.GetTokenAsync(_authRequest, CancellationToken.None);

        // Act
        var cachedToken = await _tokenProvider.GetTokenAsync(_authRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(cachedToken);
        Assert.Equal("cached-token", cachedToken.BearerToken);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewToken_WhenRefreshIsSuccessful()
    {
        // Arrange
        var oldToken = new TokenResponse { BearerToken = "expired-token", ExpiryDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1 };
        var newToken = new TokenResponse { BearerToken = "new-token", ExpiryDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 };

        SetupHttpResponse(HttpMethod.Post, ApiRoutes.Auth, HttpStatusCode.OK, oldToken);
        await _tokenProvider.GetTokenAsync(_authRequest, CancellationToken.None);

        SetupHttpResponse(HttpMethod.Get, ApiRoutes.RefreshAuth, HttpStatusCode.OK, newToken);

        // Act
        var refreshedToken = await _tokenProvider.RefreshTokenAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(refreshedToken);
        Assert.Equal("new-token", refreshedToken.BearerToken);

        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri!.ToString().EndsWith(ApiRoutes.RefreshAuth)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowException_WhenNoTokenToRefresh()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _tokenProvider.RefreshTokenAsync(CancellationToken.None));
    }

    [Theory]
    [InlineData(3600, false)]
    [InlineData(-1, true)]
    public async Task IsTokenExpired_ShouldReturnExpectedResult(int expiryOffset, bool expectedResult)
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            BearerToken = "test-token",
            ExpiryDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiryOffset
        };

        SetupHttpResponse(HttpMethod.Post, ApiRoutes.Auth, HttpStatusCode.OK, tokenResponse);

        await _tokenProvider.GetTokenAsync(_authRequest, CancellationToken.None);

        // Act
        var result = _tokenProvider.IsTokenExpired();

        // Assert
        Assert.Equal(expectedResult, result);
    }
}