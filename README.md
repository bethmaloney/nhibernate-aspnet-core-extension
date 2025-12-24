# NHibernate.Extensions.AspNetCore

> ⚠️ **Unofficial package** - This is a community-maintained library, not affiliated with the official NHibernate project.

NHibernate integration for ASP.NET Core applications with dependency injection and Microsoft.Extensions.Logging support.

[![CI](https://github.com/bethmaloney/nhibernate-aspnet-core-extension/actions/workflows/ci.yml/badge.svg)](https://github.com/bethmaloney/nhibernate-aspnet-core-extension/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/NHibernate.Extensions.AspNetCore.svg)](https://www.nuget.org/packages/NHibernate.Extensions.AspNetCore)

## 💡 Why?

Setting up NHibernate in an ASP.NET Core application requires a fair amount of boilerplate - configuring the session factory, wiring up dependency injection, and integrating logging. I wanted it to be as simple as using EF Core.

For a minimal working example, check out the [sample project](samples/SampleWebApp).

## ✨ Features

- 🔌 `AddNHibernate()` extension method for `IServiceCollection`
- 🔄 Scoped `ISession` and `IStatelessSession` (per-request lifecycle)
- 🏭 Singleton `ISessionFactory` and `Configuration`
- 📝 NHibernate logging bridged to `Microsoft.Extensions.Logging`
- 🎯 Multi-targeting: .NET 8, 9, and 10

## 📦 Installation

```bash
dotnet add package NHibernate.Extensions.AspNetCore
```

## 🚀 Quick Start

### Basic Configuration

```csharp
builder.Services.AddNHibernate(cfg =>
{
    cfg.DataBaseIntegration(db =>
    {
        db.ConnectionString = connectionString;
        db.Dialect<SQLiteDialect>();
        db.Driver<SqliteDriver>();
    });
    cfg.AddAssembly(typeof(MyEntity).Assembly);
});
```

### With Options

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
    // Enable NHibernate logging via Microsoft.Extensions.Logging
    options.EnableLogging = builder.Environment.IsDevelopment();
});
```

### Using ISession

Inject `ISession` in your services or controllers:

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

### Using IStatelessSession

For bulk operations where change tracking isn't needed:

```csharp
public class BulkImportService(IStatelessSession statelessSession)
{
    public async Task ImportAsync(IEnumerable<Product> products)
    {
        using var transaction = statelessSession.BeginTransaction();
        foreach (var product in products)
        {
            await statelessSession.InsertAsync(product);
        }
        await transaction.CommitAsync();
    }
}
```

## 📋 Service Lifetimes

| Service | Lifetime | Description |
|---------|----------|-------------|
| `Configuration` | Singleton | NHibernate configuration |
| `ISessionFactory` | Singleton | Session factory (expensive to create) |
| `ISession` | Scoped | Per-request session with change tracking |
| `IStatelessSession` | Scoped | Per-request stateless session |

## 🎯 Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

MIT
