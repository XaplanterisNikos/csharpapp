namespace CSharpApp.Application.Products.Queries.GetProducts;

/// <summary>
/// Query to retrieve all products
/// </summary>
public sealed record GetProductsQuery : IRequest<IReadOnlyCollection<Product>>;

