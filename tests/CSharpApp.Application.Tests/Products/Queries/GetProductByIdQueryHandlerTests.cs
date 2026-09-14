namespace CSharpApp.Application.Tests.Products.Queries;

/// <summary>Tests for GetProductByIdQueryHandler.</summary>
public class GetProductByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ReturnsProduct_WhenServiceFindsIt()
	{
		// Arrange
		var expected = new Product { Id = 5, Title = "Found", Price = 10 };

		var serviceMock = new Mock<IProductsService>();
		serviceMock.Setup(s => s.GetProduct(5)).ReturnsAsync(expected);

		var handler = new GetProductByIdQueryHandler(serviceMock.Object);

		// Act
		var result = await handler.Handle(new GetProductByIdQuery(5), CancellationToken.None);

		// Assert
		Assert.Equal(expected, result);
		serviceMock.Verify(s => s.GetProduct(5), Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsNull_WhenServiceFindsNothing()
	{
		// Arrange: the service returns null (product not found).
		var serviceMock = new Mock<IProductsService>();
		serviceMock.Setup(s => s.GetProduct(It.IsAny<int>())).ReturnsAsync((Product?)null);

		var handler = new GetProductByIdQueryHandler(serviceMock.Object);

		// Act
		var result = await handler.Handle(new GetProductByIdQuery(999), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}
}
