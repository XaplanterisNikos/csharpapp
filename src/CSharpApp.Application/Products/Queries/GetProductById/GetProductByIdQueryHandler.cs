namespace CSharpApp.Application.Products.Queries.GetProductById;

/// <summary>
/// Handles <see cref="GetProductByIdQuery"/> via the products service.
/// </summary>
public sealed class GetProductByIdQueryHandler
	: IRequestHandler<GetProductByIdQuery, Product?>
{
	private readonly IProductsService _productsService;

	public GetProductByIdQueryHandler(IProductsService productsService)
	{
		_productsService = productsService;
	}

	/// <inheritdoc />
	public async Task<Product?> Handle(
		GetProductByIdQuery request,
		CancellationToken cancellationToken)
	{
		// request.Id carries the value from the query.
		return await _productsService.GetProduct(request.Id);
	}
}
