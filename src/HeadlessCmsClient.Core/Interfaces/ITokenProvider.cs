namespace HeadlessCmsClient.Core.Interfaces;

public interface ITokenProvider
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken);
}