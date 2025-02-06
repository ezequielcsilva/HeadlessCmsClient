using HeadlessCmsClient.Core.Models;

namespace HeadlessCmsClient.Core.Interfaces;

/// <summary>
/// Interface for managing authentication tokens.
/// Provides methods for retrieving, refreshing, and checking token expiration.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Retrieves an authentication token based on the provided authentication request.
    /// If a valid token is already stored, it will be returned instead of making a new request.
    /// </summary>
    /// <param name="request">The authentication request containing credentials.</param>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>
    /// A <see cref="TokenResponse"/> containing the access token and expiration details, or <c>null</c> if the request fails.
    /// </returns>
    Task<TokenResponse?> GetTokenAsync(AuthRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes the authentication token if it has expired.
    /// Throws an exception if no valid token is available for refreshing.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
    /// <returns>
    /// A new <see cref="TokenResponse"/> containing the refreshed access token and expiration details, or <c>null</c> if the refresh fails.
    /// </returns>
    Task<TokenResponse?> RefreshTokenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the current authentication token is expired.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the stored token is expired or invalid; otherwise, <c>false</c>.
    /// </returns>
    bool IsTokenExpired();
}