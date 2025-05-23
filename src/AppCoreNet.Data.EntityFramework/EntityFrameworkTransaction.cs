// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using System.Data.Entity;
// using System.Data.Entity.Infrastructure; // Already implicitly available via System.Data.Entity

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Provides a <see cref="DbContext"/> transaction scope.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP007:Don\'t dispose injected",
    Justification = "Ownership is transferred from DbContextTransactionManager.")]
public sealed class EntityFrameworkTransaction<TDbContext> : ITransaction
    where TDbContext : System.Data.Entity.DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DbContextTransaction _transaction; // This is System.Data.Entity.DbContextTransaction, name is fine
    private readonly DataProviderLogger<DbContextDataProvider<TDbContext>> _logger;
    private bool _disposed;

    /// <inheritdoc />
    public string Id => _transaction.GetHashCode().ToString("X"); // EF6 DbContextTransaction doesn't have a Guid Id. Using HashCode for logging.

    internal DbContextTransaction Transaction => _transaction;

    internal event EventHandler? TransactionFinished;

    internal EntityFrameworkTransaction(
        TDbContext dbContext,
        DbContextTransaction transaction, // This is System.Data.Entity.DbContextTransaction
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
            throw new ObjectDisposedException(nameof(EntityFrameworkTransaction<TDbContext>));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _transaction.Dispose();
        _disposed = true;
        _logger.TransactionDisposed(this);

        TransactionFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        // EF6 DbContextTransaction is IDisposable, not IAsyncDisposable
        _transaction.Dispose();
        _disposed = true;
        _logger.TransactionDisposed(this);

        // EF6 DbContextTransaction is IDisposable, not IAsyncDisposable.
        // ITransaction interface has both Dispose() and DisposeAsync().
        // We call the synchronous Dispose() here.
        Dispose();
        await Task.CompletedTask; // To match ValueTask signature
    }

    /// <inheritdoc />
    public void Commit()
    {
        EnsureNotDisposed();
        _logger.TransactionCommitting(this);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _transaction.Commit();
            _logger.TransactionCommitted(this, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception error)
        {
            _logger.TransactionCommitFailed(this, error);
            throw;
        }
    }

    /// <inheritdoc />
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        // EF6 DbContextTransaction.Commit is synchronous.
        // Forward to synchronous version.
        Commit();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Rollback()
    {
        EnsureNotDisposed();
        _logger.TransactionRollingback(this);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _transaction.Rollback();
            _logger.TransactionRolledback(this, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception error)
        {
            _logger.TransactionRollbackFailed(this, error);
            throw;
        }
    }

    /// <inheritdoc />
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // EF6 DbContextTransaction.Rollback is synchronous.
        // Forward to synchronous version.
        Rollback();
        return Task.CompletedTask;
    }
}