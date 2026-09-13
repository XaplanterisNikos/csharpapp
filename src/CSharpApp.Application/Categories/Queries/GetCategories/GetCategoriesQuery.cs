namespace CSharpApp.Application.Categories.Queries.GetCategories;

/// <summary>
/// Query to retrieve all categories
/// </summary>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyCollection<Category>>;
