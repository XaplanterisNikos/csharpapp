namespace CSharpApp.Infrastructure.Handlers;

/// <summary>
/// A <see cref="DelegatingHandler"/> that attaches the bearer token to outgoing requests
/// </summary>
public sealed class AuthenticationDelegatingHandler : DelegatingHandler
{
	#region Fields

	private const string BearerScheme = "Bearer";
	private readonly ITokenProvider _tokenProvider;

	#endregion

	#region Constructor

	/// <summary>
	/// Initilaize a new instance of the <see cref="AuthenticationDelegatingHandler"/> class.
	/// </summary>
	/// <param name="tokenProvider">Supplies and refreshes the bearer token.</param>
	public AuthenticationDelegatingHandler(ITokenProvider tokenProvider)
	{
		_tokenProvider = tokenProvider;
	}

	#endregion

	#region Override method

	/// <summary>
	/// Adds the bearer token to the request
	/// Refreshes the token once and retries
	/// </summary>
	/// <param name="request"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// Attach the current token and retry once after refreshing it if rejected.
		var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
		request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, token);

		var response = await base.SendAsync(request, cancellationToken);

		if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
		{
			response.Dispose();

			var newToken = await _tokenProvider.RefreshAsync(token, cancellationToken);
			request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, newToken);

			response = await base.SendAsync(request, cancellationToken);
		}

		return response;

	}
	#endregion
}
