namespace CSharpApp.Core.Dtos.Products;

/// <summary>
/// Request payload for creating a product, using a flat category ID as required by the external API.
/// </summary>
public sealed class CreateProductRequest
{
	/// <summary>The product title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	///<summary>The product price</summary>
	[JsonPropertyName("price")]
	public int? Price { get; set; }

	///<summary>The product description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The identifier of the category this product belongs to</summary>
	[JsonPropertyName("categoryId")]
	public int? CategoryId { get; set; }

	///<summary>The product image URLs.</summary>
	[JsonPropertyName("images")]
	public List<string> Images { get; set; } = [];
}
