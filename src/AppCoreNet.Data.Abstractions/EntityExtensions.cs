// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Collections.Generic;
using AppCoreNet.Diagnostics;

namespace AppCoreNet.Data;

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
    /// <returns><c>true</c> if the entity is transient; <c>false</c> otherwise.</returns>
    public static bool IsTransient<TId>(this IEntity<TId> entity)
    {
        Ensure.Arg.NotNull(entity);
        return EqualityComparer<TId>.Default.Equals(entity.Id, default!);
    }
}