namespace CSharpApp.Application.Products;

/// <summary>
/// Typed <see cref="HttpClient/> implementation of <see cref="IProductsService"/>.
/// The client is provides by <see cref="IHttpClientFactory"/> already configured with 
/// base address, handler lifetime and resilience policies.
/// </summary>
public class ProductsService : IProductsService
{
	#region Fields

	private readonly HttpClient _httpClient;
    private readonly RestApiSettings _restApiSettings;
    private readonly ILogger<ProductsService> _logger;

	#endregion

	#region Constructor
	/// <summary>
	/// Initializes a new instance of the <see cref="ProductsService"/> class.
	/// </summary>
	/// <param name="httpClient">The typed client injected by <see cref="IHttpClientFactory"/>.</param>
	/// <param name="restApiSettings">The external REST API settings (endpoints, credentials).</param>
	/// <param name="logger">The logger for this service.</param>
	public ProductsService(
        HttpClient httpClient,
        IOptions<RestApiSettings> restApiSettings, 
        ILogger<ProductsService> logger)
    {
        _httpClient = httpClient;
        _restApiSettings = restApiSettings.Value;
        _logger = logger;
    }

	#endregion

	#region Public methods

	/// <inheritdoc />
	public async Task<IReadOnlyCollection<Product>> GetProducts()
    {
		_logger.LogInformation("Fetching all products from the external API.");

        // Sends the request and deserializes the response in a single step.
        var products = await _httpClient.GetFromJsonAsync<List<Product>>(_restApiSettings.Products);

        // Return an empty collection when the payload is empty.
        return products is null ? [] : products.AsReadOnly();

    }

    #endregion
}