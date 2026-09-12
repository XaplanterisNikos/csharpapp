namespace CSharpApp.Core.Interfaces.Categories
{
	/// <summary>
	/// Provides read/write access to categories served by the external REST API.
	/// </summary>
	public  interface ICategoriesService
	{
		/// <summary>
		/// Retrieves all categories from the external API.
		/// </summary>
		/// <returns>A read-only collection of products. Empty if the API returns no data.</returns>
		Task<IReadOnlyCollection<Category>> GetCategories();

		/// <summary>
		/// Retrieves a single category by its identifier.
		/// </summary>
		/// <param name="id">The category identifier.</param>
		/// <returns>The matching category, or <c>null</c> if no category exists with that id.</returns>
		Task<Category?> GetCategory(int id);

		/// <summary>
		/// Creates a new category.
		/// </summary>
		/// <param name="request">The category data to create.</param>
		/// <returns>The created category as returned by the API.</returns>
		Task<Category?> CreateCategory(CreateCategoryRequest request);
	}
}
