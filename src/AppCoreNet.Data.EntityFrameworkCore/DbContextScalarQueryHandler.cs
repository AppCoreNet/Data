﻿// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Provides a base class for <see cref="DbContext"/> based query handlers which return a scalar.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/></typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
public abstract class DbContextScalarQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
    : DbContextQueryHandler<TQuery, TEntity, TResult?, TDbContext, TDbEntity>
    where TQuery : IQuery<TEntity, TResult?>
    where TEntity : class, IEntity
    where TDbContext : DbContext
    where TDbEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextScalarQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    protected DbContextScalarQueryHandler(DbContextDataProvider<TDbContext> provider, ILogger logger)
        : base(provider, logger)
    {
    }

    /// <summary>
    /// Must be implemented to apply the query to the <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}"/>.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to apply.</param>
    /// <returns>The <see cref="IQueryable{T}"/>.</returns>
    protected abstract IQueryable<TDbEntity> ApplyQuery(IQueryable<TDbEntity> queryable, TQuery query);

    /// <summary>
    /// Must be implemented to project the result from <typeparamref name="TEntity"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}"/> which must be projected.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> which is executed.</param>
    /// <returns>The projected <see cref="IQueryable{T}"/>.</returns>
    protected abstract IQueryable<TResult> ApplyProjection(IQueryable<TDbEntity> queryable, TQuery query);

    /// <inheritdoc />
    protected override async Task<TResult?> QueryResult(
        IQueryable<TDbEntity> queryable,
        TQuery query,
        CancellationToken cancellationToken)
    {
        queryable = ApplyQuery(queryable, query);
        IQueryable<TResult> result = ApplyProjection(queryable, query);
        return await result.FirstOrDefaultAsync(cancellationToken)
                           .ConfigureAwait(false);
    }
}