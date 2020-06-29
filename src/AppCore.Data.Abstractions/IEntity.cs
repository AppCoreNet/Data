// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;

namespace AppCore.Data
{
    /// <summary>
    /// Represents a entity.
    /// </summary>
    /// <typeparam name="TId">The type of the unique id.</typeparam>
    public interface IEntity<out TId>
        where TId : IEquatable<TId>
    {
        /// <summary>
        /// Gets the unique id.
        /// </summary>
        TId Id { get; }
    }
}
