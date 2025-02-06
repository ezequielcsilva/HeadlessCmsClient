using HeadlessCmsClient.Core.Constants;
using HeadlessCmsClient.Core.Models;
using HeadlessCmsClient.Infrastructure.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HeadlessCmsClient.Tests;

public class CmsClientTest
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly CmsClient _cmsClient;

    public CmsClientTest()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.example.com/")
        };
        _cmsClient = new CmsClient(httpClient);
    }

    private void SetupHttpResponse(HttpMethod method, string url, HttpStatusCode statusCode, object? responseBody = null)
    {
        var json = responseBody is not null ? JsonSerializer.Serialize(responseBody) : "";
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
    public async Task GetDocumentsMetadataAsync_ShouldReturnMetadata_WhenRequestIsSuccessful()
    {
        // Arrange
        var expectedMetadata = new[]
        {
            new DocumentMetadata { Id = Guid.NewGuid(), Title = "Doc1" },
            new DocumentMetadata { Id = Guid.NewGuid(), Title = "Doc2" }
        };

        SetupHttpResponse(HttpMethod.Get, ApiRoutes.GetDocuments, HttpStatusCode.OK, expectedMetadata);

        // Act
        var result = await _cmsClient.GetDocumentsMetadataAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Length);
        Assert.Equal(expectedMetadata[0].Id, result[0].Id);
        Assert.Equal(expectedMetadata[1].Id, result[1].Id);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnDocument_WhenRequestIsSuccessful()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var expectedDocument = new Document { Id = documentId, Title = "Test Document", Content = "Some content" };

        SetupHttpResponse(HttpMethod.Get, string.Format(ApiRoutes.GetDocument, documentId), HttpStatusCode.OK, expectedDocument);

        // Act
        var result = await _cmsClient.GetDocumentAsync(documentId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentId, result.Id);
        Assert.Equal("Test Document", result.Title);
    }

    [Fact]
    public async Task GetDocumentAsync_ShouldReturnNull_WhenResponseIsNotFound()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        SetupHttpResponse(HttpMethod.Get, string.Format(ApiRoutes.GetDocument, documentId), HttpStatusCode.NotFound);

        // Act
        var result = await _cmsClient.GetDocumentAsync(documentId, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateOrUpdateDocumentAsync_ShouldSendRequest_WhenRequestIsSuccessful()
    {
        // Arrange
        var document = new Document { Id = Guid.NewGuid(), Title = "New Document", Content = "New content" };

        SetupHttpResponse(HttpMethod.Post, ApiRoutes.CreateOrUpdateDocument, HttpStatusCode.OK);

        // Act
        await _cmsClient.CreateOrUpdateDocumentAsync(document, CancellationToken.None);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().EndsWith(ApiRoutes.CreateOrUpdateDocument)),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CreateOrUpdateDocumentAsync_ShouldThrowException_WhenRequestFails()
    {
        // Arrange
        var document = new Document { Id = Guid.NewGuid(), Title = "New Document", Content = "New content" };

        SetupHttpResponse(HttpMethod.Post, ApiRoutes.CreateOrUpdateDocument, HttpStatusCode.BadRequest);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _cmsClient.CreateOrUpdateDocumentAsync(document, CancellationToken.None));
    }
}