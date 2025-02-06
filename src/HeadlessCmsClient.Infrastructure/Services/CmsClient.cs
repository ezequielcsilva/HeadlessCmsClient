using HeadlessCmsClient.Core.Constants;
using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using System.Text;
using System.Text.Json;

namespace HeadlessCmsClient.Infrastructure.Services;

internal sealed class CmsClient : ICmsClient
{
    private readonly HttpClient _httpClient;

    public CmsClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<DocumentMetadata[]> GetDocumentsMetadataAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(ApiRoutes.GetDocuments, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = JsonSerializer.Deserialize<DocumentMetadata[]>(
            await response.Content.ReadAsStringAsync(cancellationToken));

        return data ?? [];
    }

    public async Task<Document?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(string.Format(ApiRoutes.GetDocument, documentId), cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<Document>(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task CreateOrUpdateDocumentAsync(Document document, CancellationToken cancellationToken)
    {
        var content = new StringContent(JsonSerializer.Serialize(document), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(ApiRoutes.CreateOrUpdateDocument, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}