namespace CSharpApp.Application.Tests.Categories.Commands;

public class CreateCategoryCommandValidatorTests
{
	private readonly CreateCategoryCommandValidator _validator = new();

	[Fact]
	public void Validation_Passes_ForValidCommand()
	{
		var command = new CreateCategoryCommand(new CreateCategoryRequest
		{
			Name = "Valid",
			Image = "https://placehold.co/600x400"
		});

		var result = _validator.Validate(command);

		Assert.True(result.IsValid);
	}

	[Fact]
	public void Validation_Fails_WhenImageIsNotAUrl()
	{
		var command = new CreateCategoryCommand(new CreateCategoryRequest
		{
			Name = "Valid",
			Image = "not-a-url"
		});

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Validation_Fails_WhenNameIsEmpty()
	{
		var command = new CreateCategoryCommand(new CreateCategoryRequest
		{
			Name = "",
			Image = "https://placehold.co/600x400"
		});

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}
}
