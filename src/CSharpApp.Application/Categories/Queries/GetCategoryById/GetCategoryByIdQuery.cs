namespace CSharpApp.Application.Categories.Queries.GetCategoryById;

/// <summary>
/// Query to retrieve a single category by its id.
/// </summary>
public sealed record GetCategoryByIdQuery(int Id) : IRequest<Category?>;
