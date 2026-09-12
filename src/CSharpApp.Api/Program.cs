
using CSharpApp.Core.Dtos.Categories;
using CSharpApp.Core.Interfaces.Categories;
using CSharpApp.Core.Interfaces.Products;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Logging.ClearProviders().AddSerilog(logger);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDefaultConfiguration(builder.Configuration);
builder.Services.AddHttpConfiguration(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

var versionedEndpointRouteBuilder = app.NewVersionedApi();

#region Product EndPoints

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getproducts", async (IProductsService productsService) =>
    {
        var products = await productsService.GetProducts();
        return products;
    })
    .WithName("GetProducts")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getproduct/{id:int}",async (int id, IProductsService productService) =>
    {
        var product = await productService.GetProduct(id);

		// Expected "not found" → clean 404; otherwise 200 with the product.
		return product is null ? Results.NotFound() : Results.Ok(product);
    })
    .WithName("GetProduct")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapPost("api/v{version:apiVersion}/createproduct", async (CreateProductRequest request, IProductsService productService) =>
    {
        var created = await productService.CreateProduct(request);

		// Return 201 Created when a resource is successfully created.
		return Results.Created($"api/v{{version:apiVersion}}/getproduct/{created?.Id}", created);
    })
    .WithName("CreateProduct")
    .HasApiVersion(1.0);

#endregion

#region Category EndPoints

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getcategories", async (ICategoriesService categoriesService) =>
    {
        var categories = await categoriesService.GetCategories();
        return categories;
    })
    .WithName("GetCategories")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapGet("api/v{version:apiVersion}/getcategory/{id:int}", async (int id,ICategoriesService categoryService) =>
    {
        var category = await categoryService.GetCategory(id);
        return category is null ? Results.NotFound() : Results.Ok(category);
    })
    .WithName("GetCategory")
    .HasApiVersion(1.0);

versionedEndpointRouteBuilder.MapPost("api/v{version:apiVersion}/createcategory", async (CreateCategoryRequest request, ICategoriesService categoriesService) =>
    {
        var created = await categoriesService.CreateCategory(request);
        return Results.Created($"api/v{{version:apiVersion}}/getcategory/{created?.Id}", created);
    })
    .WithName("CreateCategory")
    .HasApiVersion(1.0);

#endregion

app.Run();