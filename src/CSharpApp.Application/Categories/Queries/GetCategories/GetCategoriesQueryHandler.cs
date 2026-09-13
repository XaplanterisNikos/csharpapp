namespace CSharpApp.Application.Categories.Queries.GetCategories;

/// <summary>
/// Handles <see cref="GetCategoriesQuery"/> by delegating to the category service (gateway).
/// </summary>
public sealed class GetCategoriesQueryHandler
	: IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<Category>>
{
	private readonly ICategoriesService _categoriesService;

	public GetCategoriesQueryHandler(ICategoriesService categoriesService)
	{
		_categoriesService = categoriesService;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyCollection<Category>> Handle(
		GetCategoriesQuery request,
		CancellationToken cancellationToken)
	{
		return await _categoriesService.GetCategories();
	}
}