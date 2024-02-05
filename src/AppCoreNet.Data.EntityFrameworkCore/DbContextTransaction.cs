// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Provides a <see cref="DbContext"/> transaction scope.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don\'t dispose injected",
    Justification = "Ownership is transferred from DbContextTransactionManager.")]
public sealed class DbContextTransaction<TDbContext> : ITransaction
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly IDbContextTransaction _transaction;
    private readonly DataProviderLogger<DbContextDataProvider<TDbContext>> _logger;
    private bool _disposed;

    /// <inheritdoc />
    public string Id => _transaction.TransactionId.ToString("N");

    internal IDbContextTransaction Transaction => _transaction;

    internal event EventHandler? TransactionFinished;

    internal DbContextTransaction(
        TDbContext dbContext,
        IDbContextTransaction transaction,
        DataProviderLogger<DbContextDataProvider<TDbContext>> logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(transaction);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _transaction = transaction;
        _logger = logger;
        _logger.TransactionCreated(this);
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DbContextTransaction<TDbContext>));
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

        _logger.TransactionCommitted(this, 0);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        await _transaction.CommitAsync(cancellationToken)
                          .ConfigureAwait(false);

        await DisposeAsync()
            .ConfigureAwait(false);

        _logger.TransactionCommitted(this, 0);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        EnsureNotDisposed();

        _transaction.Rollback();
        Dispose();

        _logger.TransactionRolledback(this);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        await _transaction.RollbackAsync(cancellationToken)
                          .ConfigureAwait(false);

        await DisposeAsync()
            .ConfigureAwait(false);

        _logger.TransactionRolledback(this);
    }
}