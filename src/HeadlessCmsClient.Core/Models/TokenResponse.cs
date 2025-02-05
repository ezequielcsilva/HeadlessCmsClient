namespace HeadlessCmsClient.Core.Models;

public class TokenResponse
{
    public string BearerToken { get; set; } = string.Empty;
    public int ExpiryDate { get; set; }
}