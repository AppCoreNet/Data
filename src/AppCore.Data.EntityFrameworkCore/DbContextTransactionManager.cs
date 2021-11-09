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

namespace AppCore.Data.EntityFrameworkCore
{
    public sealed class DbContextTransactionManager : ITransactionManager
    {
        private readonly IDbContextDataProvider _provider;
        private readonly ILogger<DbContextDataProvider> _logger;
        private DbContextTransaction _currentTransaction;

        /// <inheritdoc />
        public ITransaction CurrentTransaction
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

        public DbContextTransactionManager(IDbContextDataProvider provider, ILogger<DbContextDataProvider> logger)
        {
            Ensure.Arg.NotNull(provider, nameof(provider));
            Ensure.Arg.NotNull(logger, nameof(logger));

            _provider = provider;
            _logger = logger;
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
}
