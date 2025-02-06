using HeadlessCmsClient.Core.Models;

namespace HeadlessCmsClient.Core.Interfaces;

public interface ITokenProvider
{
    Task<TokenResponse?> GetTokenAsync(AuthRequest request, CancellationToken cancellationToken);

    Task<TokenResponse?> RefreshTokenAsync(CancellationToken cancellationToken);

    bool IsTokenExpired();
}