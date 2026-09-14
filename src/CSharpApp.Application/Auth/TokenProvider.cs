using CSharpApp.Core.Dtos.Auth;

namespace CSharpApp.Application.Auth;

/// <summary>
/// Caches and refreshes the shared bearer token using a dedicated HttpClient to avoid authentication recursion.
/// </summary>
public sealed class TokenProvider : ITokenProvider
{
	#region Constants

	/// <summary>Name of the login-only client without the auth handler.</summary>
	public const string AuthClientName = "AuthClient";

	#endregion

	#region Fields

	private readonly IHttpClientFactory _httpClientFactory;
	private readonly RestApiSettings _restApiSettings;
	private readonly ILogger<TokenProvider > _logger;

	// Serializes login requests to prevent concurrent calls to /auth/login.
	private readonly SemaphoreSlim _lock = new(1, 1);
	private string? _accessToken;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="TokenProvider"/> class.
	/// </summary>
	public TokenProvider(
		IHttpClientFactory httpClientFactory,
		IOptions<RestApiSettings> restApiSettings,
		ILogger<TokenProvider> logger)
	{
		// Use IHttpClientFactory to avoid capturing a transient HttpClient in a singleton.
		_httpClientFactory = httpClientFactory;
		_restApiSettings = restApiSettings.Value;
		_logger = logger;
	}

	#endregion

	#region Public methods

	/// <inheritdoc />
	public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
	{
		// Return the cached token when available.
		if (!string.IsNullOrEmpty(_accessToken)) return _accessToken;

		await _lock.WaitAsync(cancellationToken);
		try
		{
			// Recheck the token after acquiring the lock.
			if (!string.IsNullOrEmpty(_accessToken)) return _accessToken;

			return await LoginCoreAsync(cancellationToken); 
		}
		finally
		{
			_lock.Release();
		}
	}

	/// <inheritdoc />
	public async Task<string> RefreshAsync(string failedToken, CancellationToken cancellationToken = default)
	{
		await _lock.WaitAsync(cancellationToken);
		try
		{
			if (!string.IsNullOrEmpty(_accessToken) && _accessToken != failedToken)
				return _accessToken;

			return await LoginCoreAsync(cancellationToken);
		}
		finally
		{
			_lock.Release();
		}
	}

	#endregion

	#region Private helpers
	private async Task<string> LoginCoreAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Authenticating with the external API.");

		// Use a client without the auth handler to prevent authentication recursion.
		var client = _httpClientFactory.CreateClient(AuthClientName);

		var request = new LoginRequest
		{
			Email = _restApiSettings.Username,
			Password = _restApiSettings.Password,
		};

		// Remove a leading slash so the relative path resolves against the versioned base address.
		// Auth is validated at startup (ValidateOnStart),so it is guaranteed non-null here.
		var authPath = (_restApiSettings.Auth ?? throw new InvalidOperationException(
				"RestApiSettings.Auth is not configured."))
				.TrimStart('/');

		var response = await client.PostAsJsonAsync(authPath, request, cancellationToken);
		response.EnsureSuccessStatusCode();

		var login = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
		if (login?.AccessToken is null) 
			throw new InvalidOperationException("Login succeeded but not access token was returned");

		_accessToken = login.AccessToken;
		return _accessToken;
	}
	#endregion
}
