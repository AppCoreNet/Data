// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data;

/// <summary>
/// Exception which is thrown when one or more entities could not be updated.
/// </summary>
public class EntityUpdateException : EntityException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception.</param>
    public EntityUpdateException(Exception? innerException)
        : base("One or more entities could not be updated.", innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityUpdateException"/> class.
    /// </summary>
    public EntityUpdateException()
        : this(null)
    {
    }
}