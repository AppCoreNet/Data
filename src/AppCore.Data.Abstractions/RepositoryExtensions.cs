// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;

namespace AppCore.Data
{
    /// <summary>
    /// Provides extension methods for the <see cref="IRepository{TId,TEntity}"/> interface.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Loads the entity with the specified unique identifier.
        /// </summary>
        /// <typeparam name="TId">The type of the id.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The entity id.</param>
        /// <param name="cancellationToken">Can be used to cancel the asynchronous operation.</param>
        /// <exception cref="EntityNotFoundException">The entity was not found.</exception>
        /// <returns>The entity.</returns>
        public static async Task<TEntity> LoadAsync<TId, TEntity>(
            this IRepository<TId, TEntity> repository,
            TId id,
            CancellationToken cancellationToken = default
        )
            where TId : IEquatable<TId>
            where TEntity : class, IEntity<TId>
        {
            Ensure.Arg.NotNull(repository, nameof(repository));
            Ensure.Arg.NotNull(id, nameof(id));

            TEntity entity = await repository.FindAsync(id, cancellationToken);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity), id);
            }

            return entity;
        }
    }
}
