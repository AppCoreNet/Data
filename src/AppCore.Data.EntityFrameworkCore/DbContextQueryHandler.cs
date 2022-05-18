// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    // just for the logger
    internal abstract class DbContextQueryHandler {}

    /// <summary>
    /// Provides a base class for <see cref="DbContext"/> based query handlers.
    /// </summary>
    /// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
    /// <typeparam name="TEntity">The type of the <see cref="IEntity"/></typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
    public abstract class DbContextQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
        : IDbContextQueryHandler<TEntity, TResult>
        where TQuery : IQuery<TEntity, TResult>
        where TEntity : IEntity
        where TDbContext : DbContext
        where TDbEntity : class
    {
        Type IDbContextQueryHandler<TEntity, TResult>.QueryType => typeof(TQuery);

        /// <summary>
        /// Gets the <see cref="IDbContextDataProvider{TDbContext}"/> used by the query.
        /// </summary>
        protected IDbContextDataProvider<TDbContext> Provider { get; }

        /// <summary>
        /// The <see cref="ILogger"/>.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IDbContextDataProvider{TDbContext}"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        protected DbContextQueryHandler(IDbContextDataProvider<TDbContext> provider, ILoggerFactory loggerFactory)
        {
            Ensure.Arg.NotNull(provider, nameof(provider));
            Ensure.Arg.NotNull(loggerFactory, nameof(loggerFactory));

            Provider = provider;
            Logger = loggerFactory.CreateLogger<DbContextQueryHandler>();
        }

        /// <summary>
        /// Gets the <see cref="IQueryable{T}"/> of the database entity.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
        /// <returns>The queryable.</returns>
        protected virtual ValueTask<IQueryable<TDbEntity>> GetQueryable(
            TQuery query,
            CancellationToken cancellationToken)
        {
            IQueryable<TDbEntity> queryable =
                Provider.GetContext()
                        .Set<TDbEntity>()
                        .AsNoTracking();

            return new ValueTask<IQueryable<TDbEntity>>(queryable);
        }

        /// <summary>
        /// Must be implemented to query the result.
        /// </summary>
        /// <param name="queryable">The <see cref="IQueryable{T}"/> which must be queried.</param>
        /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> for which results must be queried.</param>
        /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
        /// <returns>The result of the query.</returns>
        protected abstract Task<TResult> QueryResult(
            IQueryable<TDbEntity> queryable,
            TQuery query,
            CancellationToken cancellationToken);

        /// <summary>
        /// Executes the given query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
        /// <returns>The result of the query.</returns>
        public virtual async Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(query, nameof(query));

            IQueryable<TDbEntity> queryable = await GetQueryable(query, cancellationToken)
                .ConfigureAwait(false);

            Type queryType = query.GetType();
            Logger.QueryExecuting(queryType);

            var stopwatch = new Stopwatch();

            TResult result = await QueryResult(queryable, query, cancellationToken)
                .ConfigureAwait(false);

            Logger.QueryExecuted(queryType, stopwatch.Elapsed);
            return result;
        }

        Task<TResult> IDbContextQueryHandler<TEntity,TResult>.ExecuteAsync(IQuery<TEntity, TResult> query, CancellationToken cancellationToken)
        {
            return ExecuteAsync((TQuery)query, cancellationToken);
        }
    }
}