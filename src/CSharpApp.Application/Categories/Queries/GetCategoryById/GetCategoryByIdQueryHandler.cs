namespace CSharpApp.Application.Categories.Queries.GetCategoryById;

/// <summary>
/// Handles <see cref="GetCategoryByIdQuery"/> via the category service.
/// </summary>
public sealed class GetCategoryByIdQueryHandler
	: IRequestHandler<GetCategoryByIdQuery, Category?>
{
	private readonly ICategoriesService _categoriesService;

	public GetCategoryByIdQueryHandler(ICategoriesService categoriesService)
	{
		_categoriesService = categoriesService;
	}

	/// <inheritdoc />
	public async Task<Category?> Handle(
		GetCategoryByIdQuery request,
		CancellationToken cancellationToken)
	{
		return await _categoriesService.GetCategory(request.Id);
	}
}
