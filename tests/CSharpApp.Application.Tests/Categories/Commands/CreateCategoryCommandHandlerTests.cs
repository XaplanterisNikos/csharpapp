namespace CSharpApp.Application.Tests.Categories.Commands;

/// <summary>Tests for CreateCategoryCommandHandler.</summary>
public class CreateCategoryCommandHandlerTests
{
	[Fact]
	public async Task Handle_CreatesCategory_ViaTheService()
	{
		var request = new CreateCategoryRequest
		{
			Name = "New Cat",
			Image = "https://placehold.co/600x400"
		};
		var created = new Category { Id = 50, Name = "New Cat", Image = request.Image };

		var serviceMock = new Mock<ICategoriesService>();
		serviceMock.Setup(s => s.CreateCategory(request)).ReturnsAsync(created);

		var handler = new CreateCategoryCommandHandler(serviceMock.Object);

		var result = await handler.Handle(new CreateCategoryCommand(request), CancellationToken.None);

		Assert.Equal(created, result);
		serviceMock.Verify(s => s.CreateCategory(request), Times.Once);
	}
}
