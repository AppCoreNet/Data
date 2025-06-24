// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Represents a <see cref="System.Data.Entity.DbContext"/> based query handler.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
public interface IDbContextQueryHandler<TDbContext>
    where TDbContext : DbContext
{
}

/// <summary>
/// Represents a handler for <see cref="IQuery{TEntity,TResult}"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
public interface IDbContextQueryHandler<TEntity, TResult, TDbContext> : IDbContextQueryHandler<TDbContext>
    where TEntity : class, IEntity
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets a value indicating whether the query can be executed.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to test.</param>
    /// <returns><c>true</c> if the query can be executed; otherwise, <c>false</c>.</returns>
    bool CanExecute(IQuery<TEntity, TResult> query);

    /// <summary>
    /// Executes the given query.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to execute.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The result of the query.</returns>
    Task<TResult> ExecuteAsync(IQuery<TEntity, TResult> query, CancellationToken cancellationToken = default);
}