using MsLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using MsLoggerFactoryExtensions = Microsoft.Extensions.Logging.LoggerFactoryExtensions;
using INHibernateLogger = NHibernate.INHibernateLogger;
using INHibernateLoggerFactory = NHibernate.INHibernateLoggerFactory;

namespace NHibernate.Extensions.AspNetCore.Logging;

/// <summary>
/// Factory that creates NHibernate loggers backed by Microsoft.Extensions.Logging.
/// </summary>
internal sealed class MicrosoftLoggerFactory : INHibernateLoggerFactory
{
    private readonly MsLoggerFactory _loggerFactory;

    public MicrosoftLoggerFactory(MsLoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public INHibernateLogger LoggerFor(string keyName)
    {
        return new MicrosoftLogger(_loggerFactory.CreateLogger(keyName));
    }

    public INHibernateLogger LoggerFor(System.Type type)
    {
        return new MicrosoftLogger(MsLoggerFactoryExtensions.CreateLogger(_loggerFactory, type));
    }
}
