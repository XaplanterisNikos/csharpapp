namespace CSharpApp.Application.Products.Queries.GetProductById;

/// <summary>
/// Query to retrieve a single product by its id.
/// </summary>
public sealed record GetProductByIdQuery(int Id) : IRequest<Product?>;
