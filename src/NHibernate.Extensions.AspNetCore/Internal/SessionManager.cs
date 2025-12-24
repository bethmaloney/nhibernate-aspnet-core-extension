using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NHibernate.Extensions.AspNetCore.Internal;

/// <summary>
/// Manages the lifecycle of NHibernate sessions within a DI scope.
/// </summary>
internal sealed class SessionManager(ISessionFactory sessionFactory, IOptions<NHibernateOptions> options, ILogger<SessionManager> logger) : IDisposable
{
    private readonly ISessionFactory _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
    private readonly NHibernateOptions _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<SessionManager> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly object _sessionLock = new();
    private readonly object _statelessSessionLock = new();

    private ISession? _session;
    private IStatelessSession? _statelessSession;
    private bool _disposed;

    /// <summary>
    /// Gets or creates the scoped session.
    /// </summary>
    public ISession Session
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_session is not null)
                return _session;

            lock (_sessionLock)
            {
                return _session ??= _sessionFactory.OpenSession();
            }
        }
    }

    /// <summary>
    /// Gets or creates the scoped stateless session.
    /// </summary>
    public IStatelessSession StatelessSession
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_statelessSession is not null)
                return _statelessSession;

            lock (_statelessSessionLock)
            {
                return _statelessSession ??= _sessionFactory.OpenStatelessSession();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_session is not null)
        {
            if (_options.AutoFlushOnDispose && _session.IsOpen)
            {
                try
                {
                    _session.Flush();
                }
                catch (Exception ex)
                {
                    // Swallow flush exceptions during dispose to avoid masking original exceptions
                    _logger.LogError(ex, "Error flushing NHibernate session during dispose");
                }
            }

            _session.Dispose();
            _session = null;
        }

        _statelessSession?.Dispose();
        _statelessSession = null;
    }
}
