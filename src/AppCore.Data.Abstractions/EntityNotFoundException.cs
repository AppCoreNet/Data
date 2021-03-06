// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using AppCore.Diagnostics;

namespace AppCore.Data
{
    /// <summary>
    /// Exception which is thrown when a entity was not found.
    /// </summary>
    public class EntityNotFoundException : EntityException
    {
        /// <summary>
        /// Gets the <see cref="Type"/> of the entity which was not found.
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Gets the unique id of the entity which was not found.
        /// </summary>
        public object Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="entityType">The <see cref="Type"/> of the entity.</param>
        /// <param name="id">The id of the entity.</param>
        public EntityNotFoundException(Type entityType, object id)
            : base($"Entity '{entityType.GetDisplayName()}' with id '{id}' was not found.")
        {
            Ensure.Arg.NotNull(entityType, nameof(entityType));
            Ensure.Arg.NotNull(id, nameof(id));

            EntityType = entityType;
            Id = id;
        }
    }
}