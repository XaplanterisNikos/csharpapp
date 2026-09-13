namespace CSharpApp.Core.Dtos.Auth;

/// <summary>
/// Response from POST /auth/login 
/// The access token used on protected calls and refresh token.
/// </summary>
public sealed class LoginResponse
{
	/// <summary>The bearer token </summary>
	[JsonPropertyName("access_token")]
	public string? AccessToken { get; set; }

	/// <summary>The refresh token</summary>
	[JsonPropertyName("refresh_token")]
	public string? RefreshToken { get; set; }
}
