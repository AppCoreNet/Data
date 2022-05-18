// Licensed under the MIT License.
// Copyright (c) 2020-2022 the AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides a <see cref="DbContext"/> transaction scope.
    /// </summary>
    public sealed class DbContextTransaction : ITransaction
    {
        private readonly ILogger _logger;

        private DbContext DbContext { get; }

        internal IDbContextTransaction Transaction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextTransaction"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="T:DbContext"/>.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="logger">The logger.</param>
        public DbContextTransaction(DbContext dbContext, IDbContextTransaction transaction, ILogger logger)
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
            _logger.TransactionDisposed(DbContext.GetType(), Transaction.TransactionId);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await Transaction.DisposeAsync();
            _logger.TransactionDisposed(DbContext.GetType(), Transaction.TransactionId);
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