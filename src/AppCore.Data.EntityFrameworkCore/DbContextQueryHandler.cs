// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Provides a base class for <see cref="DbContext"/> based query handlers.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/></typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
public abstract class DbContextQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
    : IDbContextQueryHandler<TEntity, TResult, TDbContext>
    where TQuery : IQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDbContext : DbContext
    where TDbEntity : class
{
    /// <summary>
    /// Gets the <see cref="DbContextDataProvider{TDbContext}"/> used by the query.
    /// </summary>
    protected DbContextDataProvider<TDbContext> Provider { get; }

    /// <summary>
    /// The <see cref="ILogger"/>.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    protected DbContextQueryHandler(DbContextDataProvider<TDbContext> provider, ILogger logger)
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotNull(logger);

        Provider = provider;
        Logger = logger;
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
            Provider.DbContext
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
    public virtual async Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(query);

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

    bool IDbContextQueryHandler<TEntity, TResult, TDbContext>.CanExecute(IQuery<TEntity, TResult> query)
    {
        return query is TQuery;
    }

    Task<TResult> IDbContextQueryHandler<TEntity, TResult, TDbContext>.ExecuteAsync(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken)
    {
        Ensure.Arg.NotNull(query);
        return ExecuteAsync((TQuery)query, cancellationToken);
    }
}