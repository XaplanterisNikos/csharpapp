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
}