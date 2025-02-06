using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using HeadlessCmsClient.Infrastructure.Http;
using Moq;
using Moq.Protected;
using System.Net;

namespace HeadlessCmsClient.Tests;

public class AuthHandlerTests
{
    private readonly Mock<ITokenProvider> _tokenProviderMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly AuthRequest _authRequest;
    private readonly HttpClient _httpClient;

    public AuthHandlerTests()
    {
        _tokenProviderMock = new Mock<ITokenProvider>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _authRequest = new AuthRequest { Username = "test", Password = "password" };

        var authHandler = new AuthHandler(_tokenProviderMock.Object, _authRequest)
        {
            InnerHandler = _httpMessageHandlerMock.Object
        };

        _httpClient = new HttpClient(authHandler);
    }

    private void SetupTokenProvider(string? token, bool isExpired, string? refreshedToken = null)
    {
        _tokenProviderMock
            .Setup(tp => tp.GetTokenAsync(_authRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token is not null ? new TokenResponse { BearerToken = token } : null);

        _tokenProviderMock
            .Setup(tp => tp.IsTokenExpired())
            .Returns(isExpired);

        if (refreshedToken != null)
        {
            _tokenProviderMock
                .Setup(tp => tp.RefreshTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse { BearerToken = refreshedToken });
        }
    }

    private void SetupHttpResponse(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var expectedResponse = new HttpResponseMessage(statusCode);
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);
    }

    private static HttpRequestMessage CreateRequest()
    {
        return new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
    }

    [Fact]
    public async Task SendAsync_ShouldAddAuthorizationHeader_WhenTokenIsValid()
    {
        // Arrange
        SetupTokenProvider("valid-token", isExpired: false);
        SetupHttpResponse();

        var request = CreateRequest();

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("valid-token", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_ShouldRefreshToken_WhenCurrentTokenIsExpired()
    {
        // Arrange
        SetupTokenProvider("expired-token", isExpired: true, refreshedToken: "new-token");
        SetupHttpResponse();

        var request = CreateRequest();

        // Act
        var response = await _httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("new-token", request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_ShouldThrowException_WhenTokenResponseIsNull()
    {
        // Arrange
        SetupTokenProvider(null, isExpired: true);
        SetupHttpResponse();

        var request = CreateRequest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _httpClient.SendAsync(request));
    }
}