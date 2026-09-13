namespace CSharpApp.Application.Products.Commands.CreateProduct;

/// <summary>
/// Handles <see cref="CreateProductCommand"/> via the products service.
/// </summary>
public sealed class CreateProductCommandHandler
	: IRequestHandler<CreateProductCommand, Product?>
{
	private readonly IProductsService _productsService;

	public CreateProductCommandHandler(IProductsService productsService)
	{
		_productsService = productsService;
	}

	/// <inheritdoc />
	public async Task<Product?> Handle(
		CreateProductCommand request,
		CancellationToken cancellationToken)
	{
		return await _productsService.CreateProduct(request.Request);
	}
}
