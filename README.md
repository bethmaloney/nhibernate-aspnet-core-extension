# NHibernate.Extensions.AspNetCore

NHibernate integration for ASP.NET Core applications with dependency injection and Microsoft.Extensions.Logging support.

## Features

- `AddNHibernate()` extension method for `IServiceCollection`
- Scoped `ISession` and `IStatelessSession` (per-request lifecycle)
- Singleton `ISessionFactory` and `Configuration`
- NHibernate logging bridged to `Microsoft.Extensions.Logging`

## Installation

```bash
dotnet add package NHibernate.Extensions.AspNetCore
```

## Usage

```csharp
builder.Services.AddNHibernate(options =>
{
    options.ConfigureNHibernate = cfg =>
    {
        cfg.DataBaseIntegration(db =>
        {
            db.ConnectionString = connectionString;
            db.Dialect<SQLiteDialect>();
        });
    };
    options.EnableLogging = builder.Environment.IsDevelopment();
});
```

Then inject `ISession` in your services:

```csharp
public class ProductService(ISession session)
{
    public async Task<Product?> GetAsync(int id)
        => await session.GetAsync<Product>(id);

    public async Task CreateAsync(Product product)
    {
        using var transaction = session.BeginTransaction();
        await session.SaveAsync(product);
        await transaction.CommitAsync();
    }
}
```

## Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

## License

MIT
