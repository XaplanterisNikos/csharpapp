namespace CSharpApp.Application.Tests.Categories.Queries;

/// <summary>Tests for GetCategoryByIdQueryHandler.</summary>
public class GetCategoryByIdQueryHandlerTests
{
	[Fact]
	public async Task Handle_ReturnsCategory_WhenServiceFindsIt()
	{
		var expected = new Category { Id = 3, Name = "Books", Image = "https://x/y.png" };

		var serviceMock = new Mock<ICategoriesService>();
		serviceMock.Setup(s => s.GetCategory(3)).ReturnsAsync(expected);

		var handler = new GetCategoryByIdQueryHandler(serviceMock.Object);

		var result = await handler.Handle(new GetCategoryByIdQuery(3), CancellationToken.None);

		Assert.Equal(expected, result);
		serviceMock.Verify(s => s.GetCategory(3), Times.Once);
	}

	[Fact]
	public async Task Handle_ReturnsNull_WhenServiceFindsNothing()
	{
		var serviceMock = new Mock<ICategoriesService>();
		serviceMock.Setup(s => s.GetCategory(It.IsAny<int>())).ReturnsAsync((Category?)null);

		var handler = new GetCategoryByIdQueryHandler(serviceMock.Object);

		var result = await handler.Handle(new GetCategoryByIdQuery(999), CancellationToken.None);

		Assert.Null(result);
	}
}
