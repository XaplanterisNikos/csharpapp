namespace CSharpApp.Core.Interfaces.Auth;

/// <summary>
/// Supplies and refreshes the bearer token for authenticated calls
/// </summary>
public interface ITokenProvider
{
	/// <summary>
	/// Returned a cached access token , logging in on first use.
	/// </summary>
	Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Forces a new login when token is rejected
	/// </summary>
	/// <param name="failedToken">The rejected token</param>
	/// <returns></returns>
	Task<string> RefreshAsync(string failedToken,  CancellationToken cancellationToken = default);
}
