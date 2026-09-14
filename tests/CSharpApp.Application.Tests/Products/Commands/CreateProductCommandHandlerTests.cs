namespace CSharpApp.Application.Tests.Products.Commands;

/// <summary>Tests for CreateProductCommandHandler.</summary>
public class CreateProductCommandHandlerTests
{
	[Fact]
	public async Task Handle_CreatesProduct_ViaTheService()
	{
		// Arrange
		var request = new CreateProductRequest
		{
			Title = "New",
			Price = 20,
			CategoryId = 1
		};
		var created = new Product { Id = 100, Title = "New", Price = 20 };

		var serviceMock = new Mock<IProductsService>();
		serviceMock.Setup(s => s.CreateProduct(request)).ReturnsAsync(created);

		var handler = new CreateProductCommandHandler(serviceMock.Object);

		// Act
		var result = await handler.Handle(new CreateProductCommand(request), CancellationToken.None);

		// Assert
		Assert.Equal(created, result);
		serviceMock.Verify(s => s.CreateProduct(request), Times.Once);
	}
}
