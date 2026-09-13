namespace CSharpApp.Application.Configuration;

/// <summary>
/// Registers Application-layer services
/// MediatR handlers , FluentValdation validator
/// </summary>
public static class ApplicationConfiguration
{
	/// <summary>
	/// Adds MediatR handlers from the Application assembly.
	/// </summary>
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var applicationAssembly = typeof(ApplicationConfiguration).Assembly;

		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

		services.AddValidatorsFromAssembly(applicationAssembly);                          
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
		return services;

	}
}
