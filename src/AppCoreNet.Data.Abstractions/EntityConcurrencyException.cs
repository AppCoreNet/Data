// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data;

/// <summary>
/// Exception which is thrown when entities could not be updated because of optimistic concurrency checks.
/// </summary>
public class EntityConcurrencyException : EntityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityConcurrencyException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    public EntityConcurrencyException(Exception? innerException)
        : base("Entities may have been modified or deleted since they were loaded.",
               innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityConcurrencyException"/> class.
    /// </summary>
    public EntityConcurrencyException()
        : this(null)
    {
    }
}