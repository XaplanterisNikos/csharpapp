namespace CSharpApp.Application.Tests.Categories.Queries;

/// <summary>Tests for GetCategoriesQueryHandler.</summary>
public class GetCategoriesQueryHandlerTests
{
	[Fact]
	public async Task Handle_ReturnsCategories_FromTheService()
	{
		var expected = new List<Category>
		{
			new() { Id = 1, Name = "Electronics", Image = "https://x/y.png" }
		}.AsReadOnly();

		var serviceMock = new Mock<ICategoriesService>();
		serviceMock.Setup(s => s.GetCategories()).ReturnsAsync(expected);

		var handler = new GetCategoriesQueryHandler(serviceMock.Object);

		var result = await handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

		Assert.Equal(expected, result);
		serviceMock.Verify(s => s.GetCategories(), Times.Once);
	}
}
