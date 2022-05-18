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
    /// <summary>
    /// Provides a base class for <see cref="DbContext"/> based data provider.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextDataProvider"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="DbContext"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        protected DbContextDataProvider(DbContext dbContext, ILoggerFactory loggerFactory)
        {
            Ensure.Arg.NotNull(dbContext, nameof(dbContext));
            Ensure.Arg.NotNull(loggerFactory, nameof(loggerFactory));

            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<DbContextDataProvider>();
            _transactionManager = new DbContextTransactionManager(this, loggerFactory);
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

    /// <summary>
    /// Provides a base class for <see cref="DbContext"/> based data provider.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    public abstract class DbContextDataProvider<TDbContext> : DbContextDataProvider, IDbContextDataProvider<TDbContext>
        where TDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextDataProvider{TDbContext}"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="DbContext"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        protected DbContextDataProvider(TDbContext dbContext, ILoggerFactory loggerFactory)
            : base(dbContext, loggerFactory)
        {
        }

        /// <inheritdoc />
        public new TDbContext GetContext()
        {
            return (TDbContext) base.GetContext();
        }
    }

    /// <summary>
    /// Provides a base class for <see cref="DbContext"/> based data provider.
    /// </summary>
    /// <typeparam name="TTag">The tag of the data provider.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    public sealed class DbContextDataProvider<TTag, TDbContext> : DbContextDataProvider<TDbContext>
        where TDbContext : DbContext
    {
        /// <inheritdoc />
        public override string Name => typeof(TTag).FullName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextDataProvider{TTag,TDbContext}"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="DbContext"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public DbContextDataProvider(TDbContext dbContext, ILoggerFactory loggerFactory)
            : base(dbContext, loggerFactory)
        {
        }
    }
}