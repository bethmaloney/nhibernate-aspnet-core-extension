using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Extensions.AspNetCore;
using NHibernate.Extensions.Sqlite;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using Scalar.AspNetCore;
using SampleWebApp.Entities;
using SampleWebApp.Mappings;
using ISession = NHibernate.ISession;
using ISessionFactory = NHibernate.ISessionFactory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Configure NHibernate
builder.Services.AddNHibernate(options =>
{
    options.ConfigureNHibernate = cfg =>
    {
        cfg.DataBaseIntegration(db =>
        {
            db.ConnectionString = "Data Source=sample.db";
            db.Dialect<SQLiteDialect>();
            db.Driver<SqliteDriver>();
            db.KeywordsAutoImport = Hbm2DDLKeyWords.None;
            db.LogFormattedSql = true;
        });

        // Add mappings
        var mapper = new ModelMapper();
        mapper.AddMapping<ProductMapping>();
        cfg.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());
    };

    // Enable logging in development
    // Set "NHibernate.SQL": "Debug" in appsettings.json to see SQL statements
    options.EnableLogging = builder.Environment.IsDevelopment();
});

var app = builder.Build();

// Create database schema on startup
using (var scope = app.Services.CreateScope())
{
    var sessionFactory = scope.ServiceProvider.GetRequiredService<ISessionFactory>();
    var cfg = scope.ServiceProvider.GetRequiredService<Configuration>();
    new SchemaExport(cfg).Create(false, true);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Product API endpoints
app.MapGet("/products", async (ISession session) =>
{
    return await session.Query<Product>().ToListAsync();
})
.WithName("GetProducts");

app.MapGet("/products/{id}", async (int id, ISession session) =>
{
    var product = await session.GetAsync<Product>(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProduct");

app.MapPost("/products", async (Product product, ISession session) =>
{
    await session.SaveAsync(product);
    await session.FlushAsync();
    return Results.Created($"/products/{product.Id}", product);
})
.WithName("CreateProduct");

app.MapPut("/products/{id}", async (int id, Product product, ISession session) =>
{
    var existing = await session.GetAsync<Product>(id);
    if (existing is null) return Results.NotFound();

    existing.Name = product.Name;
    existing.Price = product.Price;
    existing.Description = product.Description;
    await session.UpdateAsync(existing);
    await session.FlushAsync();
    return Results.Ok(existing);
})
.WithName("UpdateProduct");

app.MapDelete("/products/{id}", async (int id, ISession session) =>
{
    var product = await session.GetAsync<Product>(id);
    if (product is null) return Results.NotFound();

    await session.DeleteAsync(product);
    await session.FlushAsync();
    return Results.NoContent();
})
.WithName("DeleteProduct");

app.Run();
