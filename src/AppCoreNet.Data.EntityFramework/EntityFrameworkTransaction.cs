// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;

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
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly DbContextTransaction _transaction;
    private readonly DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> _logger;
    private bool _disposed;

    /// <inheritdoc />
    public string Id => _transaction.GetHashCode().ToString("X");

    internal DbContextTransaction Transaction => _transaction;

    internal event EventHandler? TransactionFinished;

    internal EntityFrameworkTransaction(
        TDbContext dbContext,
        DbContextTransaction transaction,
        DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> logger)
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
        Dispose();
        await Task.CompletedTask;
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
        Rollback();
        return Task.CompletedTask;
    }
}