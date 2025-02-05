namespace HeadlessCmsClient.Core.Models;

public class DocumentMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int PublishDate { get; set; }
    public int ExpiryDate { get; set; }
}