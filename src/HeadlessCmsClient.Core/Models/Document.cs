namespace HeadlessCmsClient.Core.Models;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public long PublishDate { get; set; }
    public long ExpiryDate { get; set; }
    public string Content { get; set; } = string.Empty;
}