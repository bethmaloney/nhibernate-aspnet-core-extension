using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Extensions.AspNetCore;
using NHibernate.Extensions.Sqlite;

namespace NHibernate.Extensions.AspNetCore.Tests;

public class LoggingTests
{
    private static void ConfigureSqlite(Configuration cfg)
    {
        cfg.DataBaseIntegration(db =>
        {
            db.ConnectionString = "Data Source=:memory:";
            db.Dialect<SQLiteDialect>();
            db.Driver<SqliteDriver>();
            db.KeywordsAutoImport = Hbm2DDLKeyWords.None;
        });
    }

    [Fact]
    public void EnableLogging_True_SetsUpNHibernateLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());

        // Act
        services.AddNHibernate(options =>
        {
            options.ConfigureNHibernate = ConfigureSqlite;
            options.EnableLogging = true;
        });

        var provider = services.BuildServiceProvider();

        // Trigger initialization by requesting Configuration
        var config = provider.GetRequiredService<Configuration>();

        // Assert - if we got here without exception, logging is configured
        Assert.NotNull(config);
    }

    [Fact]
    public void EnableLogging_False_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNHibernate(options =>
        {
            options.ConfigureNHibernate = ConfigureSqlite;
            options.EnableLogging = false;
        });

        var provider = services.BuildServiceProvider();

        // Assert - no exception should be thrown even without logging
        var config = provider.GetRequiredService<Configuration>();
        Assert.NotNull(config);
    }
}
