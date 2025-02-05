namespace HeadlessCmsClient.Core.Models;

public class DocumentMetadata
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public long PublishDate { get; set; }
    public long ExpiryDate { get; set; }
}