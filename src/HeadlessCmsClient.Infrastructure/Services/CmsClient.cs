using HeadlessCmsClient.Core.Constants;
using HeadlessCmsClient.Core.Interfaces;
using HeadlessCmsClient.Core.Models;
using System.Text;
using System.Text.Json;

namespace HeadlessCmsClient.Infrastructure.Services;

internal sealed class CmsClient(HttpClient httpClient) : ICmsClient
{
    public async Task<DocumentMetadata[]> GetDocumentsMetadataAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(ApiRoutes.GetDocuments, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = JsonSerializer.Deserialize<DocumentMetadata[]>(
            await response.Content.ReadAsStringAsync(cancellationToken));

        return data ?? [];
    }

    public async Task<Document?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(string.Format(ApiRoutes.GetDocument, documentId), cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<Document>(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task CreateOrUpdateDocumentAsync(Document document, CancellationToken cancellationToken)
    {
        var content = new StringContent(JsonSerializer.Serialize(document), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(ApiRoutes.CreateOrUpdateDocument, content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}