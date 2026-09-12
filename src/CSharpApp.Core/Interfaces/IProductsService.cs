namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Provides read/write access to products served by the external REST API.
/// </summary>
public interface IProductsService
{
    /// <summary>
    /// Retrieves all products from the external API.
    /// </summary>
    /// <returns>A read-only collection of products. Empty if the API returns no data.</returns>
    Task<IReadOnlyCollection<Product>> GetProducts();

    /// <summary>
    /// Retrieves a single product by its identifier.
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>The matching product , or <c>null</c> if no product exists with that id.</returns>
    Task<Product?> GetProduct(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product data to create.</param>
    /// <returns>The created product as returned by the API.</returns>
    Task<Product?> CreateProduct(CreateProductRequest request);
}