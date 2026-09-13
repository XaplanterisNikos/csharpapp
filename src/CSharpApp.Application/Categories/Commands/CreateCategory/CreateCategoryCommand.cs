namespace CSharpApp.Application.Categories.Commands.CreateCategory;

/// <summary>
/// Command to create a new category.
/// </summary>
public sealed record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<Category?>;
