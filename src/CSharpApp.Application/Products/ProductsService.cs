using System.Net;

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

	/// <inheritdoc />
	public async Task<Product?> GetProduct(int id)
	{
		_logger.LogInformation("Fetching product {ProductId} from the external API.", id);

		var response = await _httpClient.GetAsync($"{_restApiSettings.Products}/{id}");

		// Treat 400 or 404 responses as no result when the product does not exist.
		if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
		{
			_logger.LogInformation("Product {ProductId} was not found.", id);
			return null;
		}
		// Any other non-success status is unexpected: let it surface.
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<Product>();
	}

	/// <inheritdoc />
	public async Task<Product?> CreateProduct(CreateProductRequest request)
	{
		_logger.LogInformation("Creating a new product");

		// PostAsJsonAsync serializes the request DTO to a JSON body and POSTs it in one call.
		var response = await _httpClient.PostAsJsonAsync(_restApiSettings.Products, request);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<Product>();
	}

	

	#endregion
}