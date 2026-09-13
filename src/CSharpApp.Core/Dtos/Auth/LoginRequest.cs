namespace CSharpApp.Core.Dtos.Auth;

/// <summary>
/// Request body for POST /auth/login 
/// The external API expects an "email" field
/// which we populate from the configured Username setting.
/// </summary>
public sealed class LoginRequest
{
	/// <summary>The account email (mapped from the Username setting).</summary>
	[JsonPropertyName("email")]
	public string? Email { get; set; }

	/// <summary>The account password</summary>
	[JsonPropertyName("password")]
	public string? Password { get; set; }
}
