// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Provides a transaction manager using a <see cref="DbContext"/>.
/// </summary>
public sealed class DbContextTransactionManager : ITransactionManager
{
    private readonly IDbContextDataProvider _provider;
    private readonly ILogger<DbContextTransactionManager> _logger;
    private DbContextTransaction? _currentTransaction;

    /// <inheritdoc />
    public ITransaction? CurrentTransaction
    {
        get
        {
            DbContext dbContext = _provider.GetContext();
            DatabaseFacade database = dbContext.Database;
            if (_currentTransaction == null)
            {
                if (database.CurrentTransaction != null)
                {
                    _currentTransaction = new DbContextTransaction(dbContext, database.CurrentTransaction, _logger);
                }
            }
            else
            {
                if (database.CurrentTransaction == null
                    || database.CurrentTransaction != _currentTransaction.Transaction)
                {
                    _currentTransaction = database.CurrentTransaction != null
                        ? new DbContextTransaction(dbContext, database.CurrentTransaction, _logger)
                        : null;
                }
            }

            return _currentTransaction;
        }
    }

    /// <inheritdoc />
    public IDataProvider Provider => _provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextTransactionManager"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="IDbContextDataProvider"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    public DbContextTransactionManager(IDbContextDataProvider provider, ILoggerFactory loggerFactory)
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotNull(loggerFactory);

        _provider = provider;
        _logger = loggerFactory.CreateLogger<DbContextTransactionManager>();
    }

    /// <inheritdoc />
    public async Task<ITransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        DbContext dbContext = _provider.GetContext();
        IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return _currentTransaction = new DbContextTransaction(dbContext, transaction, _logger);
    }

    /// <inheritdoc />
    public ITransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        DbContext dbContext = _provider.GetContext();
        IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
        return _currentTransaction = new DbContextTransaction(dbContext, transaction, _logger);
    }
}