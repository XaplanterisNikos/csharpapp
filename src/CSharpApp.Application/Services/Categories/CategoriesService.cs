namespace CSharpApp.Application.Services.Categories;

public class CategoriesService : ICategoriesService
{
	#region Fields

	private readonly HttpClient _httpClient;
	private readonly RestApiSettings _restApiSettings;
	private readonly ILogger<CategoriesService> _logger;

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="CategoriesService"/> class.
	/// </summary>
	/// <param name="httpClient">The typed client injected by <see cref="IHttpClientFactory"/>.</param>
	/// <param name="restApiSettings">The external REST API settings (endpoints, credentials).</param>
	/// <param name="logger">The logger for this service.</param>
	public CategoriesService(
		HttpClient httpClient,
		IOptions<RestApiSettings> restApiSettings,
		ILogger<CategoriesService> logger)
	{
		_httpClient = httpClient;
		_restApiSettings = restApiSettings.Value;
		_logger = logger;
	}

	#endregion

	#region Public methods

	/// <inheritdoc />
	public async Task<IReadOnlyCollection<Category>> GetCategories()
	{
		_logger.LogInformation("Fetching all categories from the external API.");

		var categories = await _httpClient.GetFromJsonAsync<List<Category>>(_restApiSettings.Categories);

		return categories is null ?[] :categories.AsReadOnly();
	}

	/// <inheritdoc />
	public async Task<Category?> GetCategory(int id)
	{
		_logger.LogInformation("Fetching category {CategoryId} form the external API.", id);

		var response = await _httpClient.GetAsync($"{_restApiSettings.Categories}/{id}");
		if(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
		{
			_logger.LogInformation("Category {CategoryId} was not found.", id);
			return null;
		}

		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<Category>();
	}

	/// <inheritdoc />
	public async Task<Category?> CreateCategory(CreateCategoryRequest request)
	{
		_logger.LogInformation("Create a new category");

		var response = await _httpClient.PostAsJsonAsync(_restApiSettings.Categories, request);
		response.EnsureSuccessStatusCode();

		return await response.Content.ReadFromJsonAsync<Category>();
	}

	#endregion
}
