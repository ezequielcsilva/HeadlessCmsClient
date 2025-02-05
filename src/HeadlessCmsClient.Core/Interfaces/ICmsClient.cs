using HeadlessCmsClient.Core.Models;

namespace HeadlessCmsClient.Core.Interfaces;

public interface ICmsClient
{
    Task<DocumentMetadata[]> GetDocumentsMetadataAsync(CancellationToken cancellationToken);

    Task<Document> GetDocumentAsync(string documentId, CancellationToken cancellationToken);

    Task CreateOrUpdateDocumentAsync(Document document, CancellationToken cancellationToken);
}