namespace CSharpApp.Application.Products.Queries.GetProducts;

/// <summary>
/// Handles <see cref="GetProductsQuery"/> by delegating to the products service (gateway).
/// </summary>
public sealed class GetProductsQueryHandler 
	: IRequestHandler<GetProductsQuery, IReadOnlyCollection<Product>>
{
	private readonly IProductsService _productsService;

	public GetProductsQueryHandler(IProductsService productsService)
	{
		_productsService = productsService;
	}

	/// <inheritdoc />
	public async Task<IReadOnlyCollection<Product>> Handle(
		GetProductsQuery request,
		CancellationToken cancellationToken)
	{
		return await _productsService.GetProducts();
	}
}

