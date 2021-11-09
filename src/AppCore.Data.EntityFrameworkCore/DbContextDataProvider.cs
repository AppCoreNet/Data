// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public abstract class DbContextDataProvider : IDbContextDataProvider
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<DbContextDataProvider> _logger;
        private readonly DbContextTransactionManager _transactionManager;
        private readonly Stack<IDisposable> _pendingChanges = new Stack<IDisposable>();
        private readonly List<Action> _afterSaveCallbacks = new List<Action>();

        private class PendingChanges : IDisposable
        {
            private readonly Action<IDisposable> _disposeCallback;

            public PendingChanges(Action<IDisposable> disposeCallback)
            {
                _disposeCallback = disposeCallback;
            }

            public void Dispose()
            {
                _disposeCallback(this);
            }
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public ITransactionManager TransactionManager => _transactionManager;

        protected DbContextDataProvider(DbContext dbContext, ILogger<DbContextDataProvider> logger)
        {
            Ensure.Arg.NotNull(dbContext, nameof(dbContext));
            Ensure.Arg.NotNull(logger, nameof(logger));

            _dbContext = dbContext;
            _logger = logger;
            _transactionManager = new DbContextTransactionManager(this, logger);
        }

        /// <inheritdoc />
        public IDisposable BeginChangeScope(Action afterSaveCallback = null)
        {
            if (afterSaveCallback != null)
                _afterSaveCallbacks.Add(afterSaveCallback);

            IDisposable scope;
            _pendingChanges.Push(scope = new PendingChanges(EndChangeScope));
            return scope;
        }

        private void EndChangeScope(IDisposable scope)
        {
            if (!ReferenceEquals(_pendingChanges.Peek(), scope))
                throw new InvalidOperationException("Data provider change scope must be disposed in reverse-order.");

            _pendingChanges.Pop();
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            // if more than one change scope is active, save is a no-op
            if (_pendingChanges.Count > 1)
            {
                _logger.SaveChangesDeferred(_dbContext.GetType());
                return;
            }

            _logger.SavingChanges(_dbContext.GetType());

            int entityCount;
            try
            {
                entityCount = await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException error)
            {
                throw new EntityConcurrencyException(error);
            }
            catch (DbUpdateException error)
            {
                throw new EntityUpdateException(error);
            }

            foreach (Action callback in _afterSaveCallbacks)
            {
                callback();
            }

            _afterSaveCallbacks.Clear();
            _logger.SavedChanges(_dbContext.GetType(), entityCount);
        }

        /// <inheritdoc />
        public DbContext GetContext()
        {
            return _dbContext;
        }
    }

    public abstract class DbContextDataProvider<TDbContext> : DbContextDataProvider, IDbContextDataProvider<TDbContext>
        where TDbContext : DbContext
    {
        protected DbContextDataProvider(TDbContext dbContext, ILogger<DbContextDataProvider> logger)
            : base(dbContext, logger)
        {
        }

        /// <inheritdoc />
        public new TDbContext GetContext()
        {
            return (TDbContext) base.GetContext();
        }
    }

    public sealed class DbContextDataProvider<TTag, TDbContext> : DbContextDataProvider<TDbContext>
        where TDbContext : DbContext
    {
        /// <inheritdoc />
        public override string Name => typeof(TTag).FullName;
        
        public DbContextDataProvider(TDbContext dbContext, ILogger<DbContextDataProvider> logger)
            : base(dbContext, logger)
        {
        }
    }
}