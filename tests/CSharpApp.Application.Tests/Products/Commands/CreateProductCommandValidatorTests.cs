namespace CSharpApp.Application.Tests.Products.Commands;

/// <summary>Tests for CreateProductCommandValidator.</summary>
public class CreateProductCommandValidatorTests
{
	private readonly CreateProductCommandValidator _validator = new();

	[Fact]
	public void Validation_Passes_ForValidCommand()
	{
		var command = new CreateProductCommand(new CreateProductRequest
		{
			Title = "Valid",
			Price = 10,
			CategoryId = 1
		});

		var result = _validator.Validate(command);

		Assert.True(result.IsValid);
	}

	[Fact]
	public void Validation_Fails_WhenTitleIsEmpty()
	{
		var command = new CreateProductCommand(new CreateProductRequest
		{
			Title = "",
			Price = 10,
			CategoryId = 1
		});

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}

	[Fact]
	public void Validation_Fails_WhenPriceIsNegative()
	{
		var command = new CreateProductCommand(new CreateProductRequest
		{
			Title = "Valid",
			Price = -5,
			CategoryId = 1
		});

		var result = _validator.Validate(command);

		Assert.False(result.IsValid);
	}
}
