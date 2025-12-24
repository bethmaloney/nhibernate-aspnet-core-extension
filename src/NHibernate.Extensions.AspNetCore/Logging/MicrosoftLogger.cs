using Microsoft.Extensions.Logging;
using INHibernateLogger = NHibernate.INHibernateLogger;
using NHibernateLogLevel = NHibernate.NHibernateLogLevel;

namespace NHibernate.Extensions.AspNetCore.Logging;

/// <summary>
/// NHibernate logger implementation that bridges to Microsoft.Extensions.Logging.
/// </summary>
internal sealed class MicrosoftLogger : INHibernateLogger
{
    private readonly ILogger _logger;

    public MicrosoftLogger(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool IsEnabled(NHibernateLogLevel logLevel)
    {
        return _logger.IsEnabled(MapLogLevel(logLevel));
    }

    public void Log(NHibernateLogLevel logLevel, NHibernateLogValues state, Exception? exception)
    {
        var microsoftLogLevel = MapLogLevel(logLevel);

        if (!_logger.IsEnabled(microsoftLogLevel))
            return;

        _logger.Log(microsoftLogLevel, exception, state.Format, state.Args);
    }

    private static LogLevel MapLogLevel(NHibernateLogLevel logLevel)
    {
        return logLevel switch
        {
            NHibernateLogLevel.Trace => LogLevel.Trace,
            NHibernateLogLevel.Debug => LogLevel.Debug,
            NHibernateLogLevel.Info => LogLevel.Information,
            NHibernateLogLevel.Warn => LogLevel.Warning,
            NHibernateLogLevel.Error => LogLevel.Error,
            NHibernateLogLevel.Fatal => LogLevel.Critical,
            NHibernateLogLevel.None => LogLevel.None,
            _ => LogLevel.Debug
        };
    }
}
