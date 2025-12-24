namespace NHibernate.Extensions.AspNetCore.Internal;

/// <summary>
/// Manages the lifecycle of NHibernate sessions within a DI scope.
/// </summary>
internal sealed class SessionManager(ISessionFactory sessionFactory) : IDisposable
{
    private readonly ISessionFactory _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
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

        _session?.Dispose();
        _session = null;

        _statelessSession?.Dispose();
        _statelessSession = null;
    }
}
