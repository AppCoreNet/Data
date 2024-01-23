// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

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
public sealed class DbContextTransaction : ITransaction
{
    private readonly DbContext _dbContext;
    private readonly IDbContextTransaction _transaction;
    private readonly ILogger _logger;

    internal IDbContextTransaction Transaction => _transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextTransaction"/> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="T:DbContext"/>.</param>
    /// <param name="transaction">The transaction.</param>
    /// <param name="logger">The logger.</param>
    public DbContextTransaction(DbContext dbContext, IDbContextTransaction transaction, ILogger logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(transaction);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _transaction = transaction;
        _logger = logger;
        _logger.TransactionInit(dbContext.GetType(), transaction.TransactionId);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _transaction.Dispose();
        _logger.TransactionDisposed(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync()
                          .ConfigureAwait(false);

        _logger.TransactionDisposed(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public void Commit()
    {
        _transaction.Commit();
        _dbContext.ChangeTracker.AcceptAllChanges();

        _logger.TransactionCommit(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken)
                          .ConfigureAwait(false);

        _dbContext.ChangeTracker.AcceptAllChanges();

        _logger.TransactionCommit(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        _transaction.Rollback();
        _logger.TransactionRollback(_dbContext.GetType(), _transaction.TransactionId);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken)
                         .ConfigureAwait(false);

        _logger.TransactionRollback(_dbContext.GetType(), _transaction.TransactionId);
    }
}