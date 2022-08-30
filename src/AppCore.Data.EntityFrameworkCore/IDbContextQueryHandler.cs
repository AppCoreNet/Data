// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Represents a handler for <see cref="IQuery{TEntity,TResult}"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public interface IDbContextQueryHandler<TEntity, TResult, TDbContext>
    where TEntity : IEntity
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the type of the query which is handled.
    /// </summary>
    Type QueryType { get; }

    /// <summary>
    /// Executes the given query.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to execute.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The result of the query.</returns>
    Task<TResult> ExecuteAsync(IQuery<TEntity, TResult> query, CancellationToken cancellationToken = default);
}