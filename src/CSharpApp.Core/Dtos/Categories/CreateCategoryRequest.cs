namespace CSharpApp.Core.Dtos.Categories
{
	/// <summary>
	/// Request payload for creating a category, including its name and image URL. 
	/// </summary>
	public sealed class CreateCategoryRequest
	{
		///<summary>The category name.</summary>
		[JsonPropertyName("name")]
		public string? Name { get; set; }

		///<summary>Category image URL accepted by the external API.</summary>
		[JsonPropertyName("image")]
		public string? Image { get; set; }
	}
}
