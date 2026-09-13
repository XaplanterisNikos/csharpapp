namespace CSharpApp.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Handles <see cref="CreateCategoryCommand"/> via the category service.
/// </summary>
public sealed class CreateCategoryCommandHandler
	: IRequestHandler<CreateCategoryCommand, Category?>
{
	private readonly ICategoriesService _categoriesService;

	public CreateCategoryCommandHandler(ICategoriesService categoriesService)
	{
		_categoriesService = categoriesService;
	}

	/// <inheritdoc />
	public async Task<Category?> Handle(
		CreateCategoryCommand request,
		CancellationToken cancellationToken)
	{
		return await _categoriesService.CreateCategory(request.Request);
	}
}
