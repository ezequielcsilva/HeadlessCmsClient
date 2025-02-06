using HeadlessCmsClient.Core.Models;

namespace HeadlessCmsClient.Core.Interfaces;

/// <summary>
/// Interface for the Content Management System (CMS) client.
/// Provides methods to retrieve metadata, fetch documents, and create/update documents.
/// </summary>
public interface ICmsClient
{
    /// <summary>
    /// Retrieves a list of metadata for all available documents in the CMS.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A collection of <see cref="DocumentMetadata"/> representing the available documents.</returns>

    Task<DocumentMetadata[]> GetDocumentsMetadataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a specific document by its unique identifier.
    /// </summary>
    /// <param name="documentId">The unique identifier of the document.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>The requested <see cref="Document"/> object if found, or <c>null</c> if it does not exist.</returns>

    Task<Document?> GetDocumentAsync(Guid documentId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a document in the CMS.
    /// </summary>
    /// <param name="document">The document to create or update.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>

    Task CreateOrUpdateDocumentAsync(Document document, CancellationToken cancellationToken);
}