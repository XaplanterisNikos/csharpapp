namespace CSharpApp.Application.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that runs all registered validators for a request
/// before it reaches its handler. If validation fails, it throws before the handler runs.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
	: IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;

	/// <summary>
	/// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
	/// </summary>
	/// <param name="validators">All validators registered for <typeparamref name="TRequest"/>.</param>
	public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
	{
		_validators = validators;
	}

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken )
	{
		if(_validators.Any())
		{
			var context = new ValidationContext<TRequest>(request);

			var failures = _validators
				.Select(validator => validator.Validate(context))
				.SelectMany(result => result.Errors)
				.Where(failure => failure is not null)
				.ToList();

			if (failures.Count != 0) throw new ValidationException(failures);
		}

		return await next();
	}
}

