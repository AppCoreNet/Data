// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public sealed class DbContextTransaction : ITransaction
    {
        private readonly ILogger<DbContextDataProvider> _logger;

        private DbContext DbContext { get; }

        internal IDbContextTransaction Transaction { get; }

        public DbContextTransaction(DbContext dbContext, IDbContextTransaction transaction, ILogger<DbContextDataProvider> logger)
        {
            Ensure.Arg.NotNull(dbContext, nameof(dbContext));
            Ensure.Arg.NotNull(transaction, nameof(transaction));
            Ensure.Arg.NotNull(logger, nameof(logger));

            DbContext = dbContext;
            Transaction = transaction;

            _logger = logger;
            _logger.TransactionInit(dbContext.GetType(), transaction.TransactionId);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Transaction.Dispose();
            _logger.TransactionRollback(DbContext.GetType(), Transaction.TransactionId);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return Transaction.DisposeAsync();
        }

        /// <inheritdoc />
        public void Commit()
        {
            Transaction.Commit();
            DbContext.ChangeTracker.AcceptAllChanges();

            _logger.TransactionCommit(DbContext.GetType(), Transaction.TransactionId);
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await Transaction.CommitAsync(cancellationToken)
                .ConfigureAwait(false);

            DbContext.ChangeTracker.AcceptAllChanges();

            _logger.TransactionCommit(DbContext.GetType(), Transaction.TransactionId);
        }

        /// <inheritdoc />
        public void Rollback()
        {
            Transaction.Rollback();
            _logger.TransactionRollback(DbContext.GetType(), Transaction.TransactionId);
        }

        /// <inheritdoc />
        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await Transaction.RollbackAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.TransactionRollback(DbContext.GetType(), Transaction.TransactionId);
        }
    }
}