// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

/// <summary>
/// Represents a entity.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets the unique id.
    /// </summary>
    object Id { get; }
}

/// <summary>
/// Represents a entity.
/// </summary>
/// <typeparam name="TId">The type of the unique id.</typeparam>
public interface IEntity<out TId> : IEntity
{
    /// <summary>
    /// Gets the unique id.
    /// </summary>
    new TId Id { get; }
}