// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Provides a <see cref="DbContext"/> transaction scope.
/// </summary>
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don\'t dispose injected",
    Justification = "Ownership is transferred from DbContextTransactionManager.")]
public sealed class DbContextTransaction : ITransaction
{
    private readonly DbContext _dbContext;
    private readonly IDbContextTransaction _transaction;
    private readonly ILogger _logger;
    private bool _disposed;

    internal IDbContextTransaction Transaction => _transaction;

    internal event EventHandler? TransactionFinished;

    internal DbContextTransaction(DbContext dbContext, IDbContextTransaction transaction, ILogger logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(transaction);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _transaction = transaction;
        _logger = logger;
        _logger.TransactionInit(dbContext.GetType(), transaction.TransactionId);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbContextTransaction));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _transaction.Dispose();
        _disposed = true;

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await _transaction.DisposeAsync()
                          .ConfigureAwait(false);

        _disposed = true;

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void Commit()
    {
        EnsureNotDisposed();

        _transaction.Commit();
        Dispose();

        _logger.TransactionCommit(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        await _transaction.CommitAsync(cancellationToken)
                          .ConfigureAwait(false);

        await DisposeAsync()
            .ConfigureAwait(false);

        _logger.TransactionCommit(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        EnsureNotDisposed();

        _transaction.Rollback();
        Dispose();

        _logger.TransactionRollback(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        await _transaction.RollbackAsync(cancellationToken)
                          .ConfigureAwait(false);

        await DisposeAsync()
            .ConfigureAwait(false);

        _logger.TransactionRollback(_dbContext.GetType(), _transaction.TransactionId);
    }
}