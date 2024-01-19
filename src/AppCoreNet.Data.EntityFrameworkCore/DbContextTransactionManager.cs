// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Provides a transaction manager using a <see cref="DbContext"/>.
/// </summary>
public sealed class DbContextTransactionManager : ITransactionManager
{
    private readonly DbContext _dbContext;
    private readonly ILogger<DbContextTransactionManager> _logger;
    private DbContextTransaction? _currentTransaction;

    /// <inheritdoc />
    public ITransaction? CurrentTransaction
    {
        get
        {
            DatabaseFacade database = _dbContext.Database;
            if (_currentTransaction == null)
            {
                if (database.CurrentTransaction != null)
                {
                    _currentTransaction = new DbContextTransaction(_dbContext, database.CurrentTransaction, _logger);
                }
            }
            else
            {
                if (database.CurrentTransaction == null
                    || database.CurrentTransaction != _currentTransaction.Transaction)
                {
                    _currentTransaction = database.CurrentTransaction != null
                        ? new DbContextTransaction(_dbContext, database.CurrentTransaction, _logger)
                        : null;
                }
            }

            return _currentTransaction;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextTransactionManager"/> class.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext"/>.</param>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/>.</param>
    public DbContextTransactionManager(DbContext dbContext, ILogger<DbContextTransactionManager> logger)
    {
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(logger);

        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                                                            .ConfigureAwait(false);

        return _currentTransaction = new DbContextTransaction(_dbContext, transaction, _logger);
    }

    /// <inheritdoc />
    public ITransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        IDbContextTransaction transaction = _dbContext.Database.BeginTransaction();
        return _currentTransaction = new DbContextTransaction(_dbContext, transaction, _logger);
    }
}