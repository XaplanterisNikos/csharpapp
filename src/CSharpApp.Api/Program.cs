var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders().AddSerilog(logger);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration(builder.Configuration);
builder.Services.AddHttpConfiguration(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();

// First in the pipeline so the measured time covers the entire request.
app.UseRequestPerformanceLogging();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var versionedEndpointRouteBuilder = app.NewVersionedApi();

#region Product EndPoints

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getproducts", async (ISender mediator) =>
    {
		return await mediator.Send(new GetProductsQuery());
	})
    .WithName("GetProducts")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getproduct/{id:int}",async (int id, ISender mediator) =>
    {
		var product = await mediator.Send(new GetProductByIdQuery(id));
		return product is null ? Results.NotFound() : Results.Ok(product);
	})
    .WithName("GetProduct")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapPost("api/v{version:apiVersion}/createproduct", async (CreateProductRequest request, ISender mediator) =>
    {
		var created = await mediator.Send(new CreateProductCommand(request));
		return Results.Created($"api/v{{version:apiVersion}}/getproduct/{created?.Id}", created);
	})
    .WithName("CreateProduct")
    .HasApiVersion(1.0);

#endregion

#region Category EndPoints

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getcategories", async (ISender mediator) =>
    {
		return await mediator.Send(new GetCategoriesQuery());
	})
    .WithName("GetCategories")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getcategory/{id:int}", async (int id, ISender mediator) =>
    {
		var category = await mediator.Send(new GetCategoryByIdQuery(id));
		return category is null ? Results.NotFound() : Results.Ok(category);
	})
    .WithName("GetCategory")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapPost("api/v{version:apiVersion}/createcategory", async (CreateCategoryRequest request, ISender mediator) =>
    {
		var created = await mediator.Send(new CreateCategoryCommand(request));
		return Results.Created($"api/v{{version:apiVersion}}/getcategory/{created?.Id}", created);
	})
    .WithName("CreateCategory")
    .HasApiVersion(1.0);

#endregion

app.Run();