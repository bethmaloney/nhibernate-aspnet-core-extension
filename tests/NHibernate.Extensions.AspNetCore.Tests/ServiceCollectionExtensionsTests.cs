using Microsoft.Extensions.DependencyInjection;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Extensions.Sqlite;

namespace NHibernate.Extensions.AspNetCore.Tests;

public class ServiceCollectionExtensionsTests
{
    private static void ConfigureSqlite(Configuration cfg)
    {
        cfg.DataBaseIntegration(db =>
        {
            db.ConnectionString = "Data Source=:memory:";
            db.Dialect<SQLiteDialect>();
            db.Driver<SqliteDriver>();
            // Disable schema metadata update to avoid compatibility issues with newer Microsoft.Data.Sqlite
            db.KeywordsAutoImport = Hbm2DDLKeyWords.None;
        });
    }

    [Fact]
    public void AddNHibernate_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddNHibernate(ConfigureSqlite);

        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<Configuration>());
        Assert.NotNull(provider.GetService<ISessionFactory>());
    }

    [Fact]
    public void AddNHibernate_WithOptions_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddNHibernate(options =>
        {
            options.ConfigureNHibernate = ConfigureSqlite;
            options.EnableLogging = false;
        });

        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<Configuration>());
        Assert.NotNull(provider.GetService<ISessionFactory>());
    }

    [Fact]
    public void ISession_IsScopedPerRequest()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNHibernate(ConfigureSqlite);

        var provider = services.BuildServiceProvider();

        // Act & Assert
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var session1a = scope1.ServiceProvider.GetRequiredService<ISession>();
        var session1b = scope1.ServiceProvider.GetRequiredService<ISession>();
        var session2 = scope2.ServiceProvider.GetRequiredService<ISession>();

        // Same scope should return same session
        Assert.Same(session1a, session1b);

        // Different scopes should return different sessions
        Assert.NotSame(session1a, session2);
    }

    [Fact]
    public void IStatelessSession_IsScopedPerRequest()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNHibernate(ConfigureSqlite);

        var provider = services.BuildServiceProvider();

        // Act & Assert
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var session1a = scope1.ServiceProvider.GetRequiredService<IStatelessSession>();
        var session1b = scope1.ServiceProvider.GetRequiredService<IStatelessSession>();
        var session2 = scope2.ServiceProvider.GetRequiredService<IStatelessSession>();

        // Same scope should return same session
        Assert.Same(session1a, session1b);

        // Different scopes should return different sessions
        Assert.NotSame(session1a, session2);
    }

    [Fact]
    public void ISessionFactory_IsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNHibernate(ConfigureSqlite);

        var provider = services.BuildServiceProvider();

        // Act
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var factory1 = scope1.ServiceProvider.GetRequiredService<ISessionFactory>();
        var factory2 = scope2.ServiceProvider.GetRequiredService<ISessionFactory>();

        // Assert
        Assert.Same(factory1, factory2);
    }

    [Fact]
    public void Configuration_IsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNHibernate(ConfigureSqlite);

        var provider = services.BuildServiceProvider();

        // Act
        var config1 = provider.GetRequiredService<Configuration>();
        var config2 = provider.GetRequiredService<Configuration>();

        // Assert
        Assert.Same(config1, config2);
    }
}
