namespace CSharpApp.Application.Products.Commands.CreateProduct.Validator;

/// <summary>
/// Validation rules for <see cref="CreateProductCommand"/>.
/// </summary>
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
	public CreateProductCommandValidator()
	{
		RuleFor(c => c.Request.Title)
			.NotEmpty().WithMessage("Title is required.");

		RuleFor(c => c.Request.Price)
			.NotNull().WithMessage("Price is required.")
			.GreaterThanOrEqualTo(0).WithMessage("Price must be zero or greater.");

		RuleFor(c => c.Request.CategoryId)
			.NotNull().WithMessage("CategoryId is required.")
			.GreaterThan(0).WithMessage("CategoryId must be greater than zero.");
	}
}
