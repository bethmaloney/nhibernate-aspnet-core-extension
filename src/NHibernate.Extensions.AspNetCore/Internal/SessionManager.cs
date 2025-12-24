using Microsoft.Extensions.Options;

namespace NHibernate.Extensions.AspNetCore.Internal;

/// <summary>
/// Manages the lifecycle of NHibernate sessions within a DI scope.
/// </summary>
internal sealed class SessionManager : IDisposable
{
    private readonly ISessionFactory _sessionFactory;
    private readonly NHibernateOptions _options;
    private readonly object _sessionLock = new();
    private readonly object _statelessSessionLock = new();

    private ISession? _session;
    private IStatelessSession? _statelessSession;
    private bool _disposed;

    public SessionManager(ISessionFactory sessionFactory, IOptions<NHibernateOptions> options)
    {
        _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

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
                catch
                {
                    // Swallow flush exceptions during dispose to avoid masking original exceptions
                }
            }

            _session.Dispose();
            _session = null;
        }

        if (_statelessSession is not null)
        {
            _statelessSession.Dispose();
            _statelessSession = null;
        }
    }
}
