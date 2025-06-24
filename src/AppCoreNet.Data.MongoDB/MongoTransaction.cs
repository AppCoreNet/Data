// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Represents a MongoDB transaction.
/// </summary>
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don\'t dispose injected",
    Justification = "Ownership is transferred from MongoTransactionManager.")]
public sealed class MongoTransaction : ITransaction
{
    private readonly IClientSessionHandle _session;
    private readonly string _id;
    private readonly DataProviderLogger<MongoDataProvider> _logger;
    private bool _disposed;

    /// <inheritdoc />
    public string Id => _id;

    /// <summary>
    /// Gets the <see cref="IClientSessionHandle"/> of the transaction.
    /// </summary>
    public IClientSessionHandle SessionHandle => _session;

    internal event EventHandler? TransactionFinished;

    internal MongoTransaction(IClientSessionHandle session, DataProviderLogger<MongoDataProvider> logger)
    {
        _session = session;
        _logger = logger;

        BsonBinaryData sessionIdBytes = session.ServerSession.Id["id"].AsBsonBinaryData;
        _id = BitConverter.ToString(sessionIdBytes.Bytes)
                          .Replace("-", string.Empty);

        _logger.TransactionCreated(this);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MongoTransaction));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _session.Dispose();
        _disposed = true;
        _logger.TransactionDisposed(this);

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await Task.Run(() => _session.Dispose())
                  .ConfigureAwait(false);

        _disposed = true;
        _logger.TransactionDisposed(this);

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Commit()
    {
        EnsureNotDisposed();

        _logger.TransactionCommitting(this);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            _session.CommitTransaction();
            _logger.TransactionCommitted(this, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception error)
        {
            _logger.TransactionCommitFailed(this, error);
            throw;
        }

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        EnsureNotDisposed();

        _logger.TransactionRollingback(this);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            _session.AbortTransaction();
            _logger.TransactionRolledback(this, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception error)
        {
            _logger.TransactionRollbackFailed(this, error);
            throw;
        }

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        _logger.TransactionCommitting(this);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _session.CommitTransactionAsync(cancellationToken)
                          .ConfigureAwait(false);

            _logger.TransactionCommitted(this, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception error)
        {
            _logger.TransactionCommitFailed(this, error);
            throw;
        }

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        _logger.TransactionRollingback(this);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _session.AbortTransactionAsync(cancellationToken)
                          .ConfigureAwait(false);

            _logger.TransactionRolledback(this, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception error)
        {
            _logger.TransactionRollbackFailed(this, error);
            throw;
        }

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }
}