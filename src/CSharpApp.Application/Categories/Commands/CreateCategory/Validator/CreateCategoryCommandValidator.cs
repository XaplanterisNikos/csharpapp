namespace CSharpApp.Application.Categories.Commands.CreateCategory.Validator;

/// <summary>
/// Validation rules for <see cref="CreateCategoryCommand"/>.
/// </summary>
public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
	public CreateCategoryCommandValidator()
	{
		RuleFor(c => c.Request.Name)
			.NotEmpty().WithMessage("Name is required.");

		// The external API rejects a category whose image is not a valid URL 
		RuleFor(c => c.Request.Image)
			.NotEmpty().WithMessage("Image is required.")
			.Must(BeAValidUrl).WithMessage("Image must be a valid URL.");
	}

	private static bool BeAValidUrl(string? url) =>
		Uri.TryCreate(url, UriKind.Absolute, out var result)
		&& (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
}
