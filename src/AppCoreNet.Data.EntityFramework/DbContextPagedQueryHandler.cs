// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Provides a base class for <see cref="DbContext"/> based query handlers which return a page of the result set.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
public abstract class DbContextPagedQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
    : DbContextQueryHandler<TQuery, TEntity, IPagedResult<TResult>, TDbContext, TDbEntity>
    where TQuery : IPagedQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDbContext : DbContext
    where TDbEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextPagedQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
    protected DbContextPagedQueryHandler(DbContextDataProvider<TDbContext> provider)
        : base(provider)
    {
    }

    /// <summary>
    /// Must be implemented to apply the query to the <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}"/>.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to apply.</param>
    /// <returns>The <see cref="IQueryable{T}"/> with the query applied.</returns>
    protected abstract IQueryable<TDbEntity> ApplyQuery(IQueryable<TDbEntity> queryable, TQuery query);

    /// <summary>
    /// Must be implemented to project the result from <typeparamref name="TEntity"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> which must be projected.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> which is executed.</param>
    /// <returns>The projected <see cref="IQueryable{T}"/>.</returns>
    protected abstract IQueryable<TResult> ApplyProjection(IQueryable<TDbEntity> queryable, TQuery query);

    /// <summary>
    /// Can be overriden to customize the paging.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> which must be paged.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> which is executed.</param>
    /// <returns>The projected <see cref="IQueryable{T}"/>.</returns>
    protected virtual IQueryable<TDbEntity> ApplyPaging(IQueryable<TDbEntity> queryable, TQuery query)
    {
        if (query.Offset > 0)
            queryable = queryable.Skip((int)query.Offset);

        return queryable.Take(query.Limit);
    }

    /// <inheritdoc />
    protected override async Task<IPagedResult<TResult>> QueryResultAsync(
        IQueryable<TDbEntity> queryable,
        TQuery query,
        CancellationToken cancellationToken)
    {
        queryable = ApplyQuery(queryable, query);

        long? totalCount = query.TotalCount
            ? await queryable.LongCountAsync(cancellationToken)
                             .ConfigureAwait(false)
            : null;

        queryable = ApplyPaging(queryable, query);
        IQueryable<TResult> projectedQueryable = ApplyProjection(queryable, query);

        TResult[] result = await projectedQueryable.ToArrayAsync(cancellationToken)
                                                   .ConfigureAwait(false);

        return new PagedResult<TResult>(result, totalCount);
    }
}