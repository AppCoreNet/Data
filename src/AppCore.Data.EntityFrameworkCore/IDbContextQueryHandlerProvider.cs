// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;

namespace AppCore.Data.EntityFrameworkCore
{
    /// <summary>
    /// Represents a provider for resolving <see cref="IDbContextQueryHandler{TEntity,TResult}"/>.
    /// </summary>
    public interface IDbContextQueryHandlerProvider
    {
        /// <summary>
        /// Gets the query handler implementation for the specified <paramref name="queryType"/>.
        /// </summary>
        /// <param name="queryType">The type of the query.</param>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The query handler instance.</returns>
        IDbContextQueryHandler<TEntity, TResult> GetHandler<TEntity, TResult>(Type queryType)
            where TEntity : IEntity;
    }
}