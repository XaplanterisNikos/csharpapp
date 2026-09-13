namespace CSharpApp.Application.Products.Commands.CreateProduct;

/// <summary>
/// Command to create a new product.
/// </summary>
public sealed record CreateProductCommand(CreateProductRequest Request) : IRequest<Product?>;
