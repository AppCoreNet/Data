﻿// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Represents a <see cref="DbContext"/> based query handler.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public interface IDbContextQueryHandler<TDbContext>
    where TDbContext : DbContext
{
}

/// <summary>
/// Represents a handler for <see cref="IQuery{TEntity,TResult}"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public interface IDbContextQueryHandler<TEntity, TResult, TDbContext> : IDbContextQueryHandler<TDbContext>
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets a value indicating whether the query can be executed.
    /// </summary>
    bool CanExecute(IQuery<TEntity, TResult> query);

    /// <summary>
    /// Executes the given query.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to execute.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The result of the query.</returns>
    Task<TResult> ExecuteAsync(IQuery<TEntity, TResult> query, CancellationToken cancellationToken = default);
}