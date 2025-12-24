using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MsLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using NHibernate.Cfg;
using NHibernate.Extensions.AspNetCore.Internal;
using NHibernate.Extensions.AspNetCore.Logging;

namespace NHibernate.Extensions.AspNetCore;

/// <summary>
/// Extension methods for configuring NHibernate services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class NHibernateServiceCollectionExtensions
{
    /// <summary>
    /// Adds NHibernate services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureNHibernate">An action to configure NHibernate.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddNHibernate(
        this IServiceCollection services,
        Action<Configuration> configureNHibernate)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureNHibernate);

        return services.AddNHibernate(options => options.ConfigureNHibernate = configureNHibernate);
    }

    /// <summary>
    /// Adds NHibernate services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">An action to configure <see cref="NHibernateOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddNHibernate(
        this IServiceCollection services,
        Action<NHibernateOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Configure options
        services.Configure(configureOptions);

        // Register Configuration as singleton
        services.TryAddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NHibernateOptions>>().Value;

            // Set up logging if enabled
            if (options.EnableLogging)
            {
                var loggerFactory = sp.GetService<MsLoggerFactory>();
                if (loggerFactory is not null)
                {
                    NHibernateLogger.SetLoggersFactory(new MicrosoftLoggerFactory(loggerFactory));
                }
            }

            var cfg = new Configuration();
            options.ConfigureNHibernate?.Invoke(cfg);
            return cfg;
        });

        // Register ISessionFactory as singleton
        services.TryAddSingleton<ISessionFactory>(sp =>
        {
            var cfg = sp.GetRequiredService<Configuration>();
            return cfg.BuildSessionFactory();
        });

        // Register SessionManager as scoped (per-request)
        services.TryAddScoped<SessionManager>();

        // Register ISession as scoped, delegating to SessionManager
        services.TryAddScoped(sp => sp.GetRequiredService<SessionManager>().Session);

        // Register IStatelessSession as scoped, delegating to SessionManager
        services.TryAddScoped(sp => sp.GetRequiredService<SessionManager>().StatelessSession);

        return services;
    }
}
