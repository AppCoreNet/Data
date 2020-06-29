// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using AppCore.Diagnostics;

namespace AppCore.Data
{
    /// <summary>
    /// Provides extension methods for the <see cref="IEntity{TId}"/> interface.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Gets a value indicating whether the entity is transient.
        /// </summary>
        /// <typeparam name="TId">The type of the entity id.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns><c>tru</c> if the entity is transient; <c>false</c> otherwise.</returns>
        public static bool IsTransient<TId>(this IEntity<TId> entity)
            where TId : IEquatable<TId>
        {
            Ensure.Arg.NotNull(entity, nameof(entity));
            return EqualityComparer<TId>.Default.Equals(entity.Id, default);
        }
    }
}