using CSharpApp.Application.Products.Queries.GetProducts;
using CSharpApp.Core.Interfaces.Products;

namespace CSharpApp.Application.Tests.Products.Queries;

/// <summary>Tests for GetProductsQueryHandler.</summary>
public class GetProductsQueryHandlerTests
{
	[Fact]
	public async Task Hande_ReturnsProducts_FromTheService()
	{
		// ---- Arrange ----
		// Fake data the mocked service will return (no real HTTP call).
		var expectedProducts = new List<Product>
		{
			new(){Id = 1,Title = "Test Product", Price = 10}
		}.AsReadOnly();

		// A stand-in for IProductsService.
		var productsServiceMock = new Mock<IProductsService>();

		// When GetProducts() is called, return our fake list.
		productsServiceMock
			.Setup(service => service.GetProducts())
			.ReturnsAsync(expectedProducts);

		// Build the handler with the mock injected instead of the real service.
		var handler = new GetProductsQueryHandler(productsServiceMock.Object);

		var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

		// ---- Assert ----
		// The handler returns exactly what the service gave it.
		Assert.Equal(expectedProducts, result);

		// And it called the service exactly once.
		productsServiceMock.Verify(service => service.GetProducts(), Times.Once);
	}
}
