// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using System.Data.Entity;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Provides a base class for <see cref="DbContext"/> based query handlers.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
public abstract class EntityFrameworkQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
    : IEntityFrameworkQueryHandler<TEntity, TResult, TDbContext>
    where TQuery : IQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDbContext : System.Data.Entity.DbContext
    where TDbEntity : class
{
    /// <summary>
    /// Gets the <see cref="EntityFrameworkDataProvider{TDbContext}"/> used by the query.
    /// </summary>
    protected EntityFrameworkDataProvider<TDbContext> Provider { get; }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EntityFrameworkQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="EntityFrameworkDataProvider{TDbContext}"/>.</param>
    protected EntityFrameworkQueryHandler(EntityFrameworkDataProvider<TDbContext> provider)
    {
        Ensure.Arg.NotNull(provider);
        Provider = provider;
    }

    /// <summary>
    /// Gets the <see cref="IQueryable{T}"/> of the database entity.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The queryable.</returns>
    protected virtual ValueTask<IQueryable<TDbEntity>> GetQueryableAsync(
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
    protected abstract Task<TResult> QueryResultAsync(
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

        IQueryable<TDbEntity> queryable = await GetQueryableAsync(query, cancellationToken)
            .ConfigureAwait(false);

        TResult result = await QueryResultAsync(queryable, query, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    bool IEntityFrameworkQueryHandler<TEntity, TResult, TDbContext>.CanExecute(IQuery<TEntity, TResult> query)
    {
        return query is TQuery;
    }

    /// <inheritdoc />
    Task<TResult> IEntityFrameworkQueryHandler<TEntity, TResult, TDbContext>.ExecuteAsync(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken)
    {
        Ensure.Arg.NotNull(query);
        return ExecuteAsync((TQuery)query, cancellationToken);
    }
}