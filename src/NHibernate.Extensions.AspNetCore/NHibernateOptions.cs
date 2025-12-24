using NHibernate.Cfg;

namespace NHibernate.Extensions.AspNetCore;

/// <summary>
/// Options for configuring NHibernate in ASP.NET Core applications.
/// </summary>
public class NHibernateOptions
{
    /// <summary>
    /// Action to configure the NHibernate <see cref="Configuration"/>.
    /// </summary>
    public Action<Configuration>? ConfigureNHibernate { get; set; }

    /// <summary>
    /// When true, integrates NHibernate logging with Microsoft.Extensions.Logging.
    /// Default is true.
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}
