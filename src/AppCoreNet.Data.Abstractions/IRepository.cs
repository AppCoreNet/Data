// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;

namespace AppCoreNet.Data;

/// <summary>
/// Represents a repository.
/// </summary>
/// <typeparam name="TId">The type of the entity id.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IRepository<in TId, TEntity>
    where TEntity : class, IEntity<TId>
{
    /// <summary>
    /// Gets the <see cref="IDataProvider"/> which owns the repository.
    /// </summary>
    IDataProvider Provider { get; }

    /// <summary>
    /// Executes the specified <paramref name="query"/> and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">The query which should be executed.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     The task representing the asynchronous operation. The result contains the result.
    /// </returns>
    Task<TResult> QueryAsync<TResult>(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tries to load the entity with the specified unique id.
    /// </summary>
    /// <param name="id">The unique id of the entity.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>
    ///     The task representing the asynchronous operation. The result contains the entity or <c>null</c> if it was not found.
    /// </returns>
    Task<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the entity with the specified unique identifier.
    /// </summary>
    /// <param name="id">The entity id.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <exception cref="EntityNotFoundException">The entity was not found.</exception>
    /// <returns>The entity.</returns>
    Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the specified entity.
    /// </summary>
    /// <param name="entity">The entity to store.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the specified entity.
    /// </summary>
    /// <param name="entity">The entity to store.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified entity.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
    /// <returns>The task representing the asynchronous operation.</returns>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}