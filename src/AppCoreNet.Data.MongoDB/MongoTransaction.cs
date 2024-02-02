// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;
    private bool _disposed;

    /// <summary>
    /// Gets the <see cref="IClientSessionHandle"/> of the transaction.
    /// </summary>
    public IClientSessionHandle SessionHandle => _session;

    internal event EventHandler? TransactionFinished;

    internal MongoTransaction(IClientSessionHandle session, ILogger logger)
    {
        _session = session;
        _logger = logger;

        BsonBinaryData sessionIdBytes = session.ServerSession.Id["id"].AsBsonBinaryData;
        _id = BitConverter.ToString(sessionIdBytes.Bytes)
                          .Replace("-", string.Empty);

        _logger.TransactionCreated(_id);
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

        if (_session.IsInTransaction)
        {
            Rollback();
        }

        TransactionFinished?.Invoke(this, EventArgs.Empty);

        _session.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        if (_session.IsInTransaction)
        {
            await RollbackAsync()
                .ConfigureAwait(false);
        }

        TransactionFinished?.Invoke(this, EventArgs.Empty);

        _session.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public void Commit()
    {
        EnsureNotDisposed();

        _logger.TransactionCommitting(_id);
        try
        {
            _session.CommitTransaction();
            _logger.TransactionCommitted(_id);
        }
        catch (Exception error)
        {
            _logger.TransactionCommitFailed(error, _id);
        }

        Dispose();
    }

    /// <inheritdoc />
    public void Rollback()
    {
        EnsureNotDisposed();

        _logger.TransactionRollingback(_id);
        try
        {
            _session.AbortTransaction();
        }
        catch
        {
            // ignored
        }

        _logger.TransactionRollback(_id);

        Dispose();
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        _logger.TransactionCommitting(_id);
        try
        {
            await _session.CommitTransactionAsync(cancellationToken)
                          .ConfigureAwait(false);

            _logger.TransactionCommitted(_id);
        }
        catch (Exception error)
        {
            _logger.TransactionCommitFailed(error, _id);
        }

        await DisposeAsync()
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        _logger.TransactionRollingback(_id);

        try
        {
            await _session.AbortTransactionAsync(cancellationToken)
                          .ConfigureAwait(false);
        }
        catch
        {
            // ignored
        }

        _logger.TransactionRollback(_id);

        await DisposeAsync()
            .ConfigureAwait(false);
    }
}