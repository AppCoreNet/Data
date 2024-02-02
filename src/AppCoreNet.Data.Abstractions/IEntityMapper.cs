// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

/// <summary>
/// Maps domain entities to/from data entities.
/// </summary>
public interface IEntityMapper
{
    /// <summary>
    /// Maps a entity.
    /// </summary>
    /// <typeparam name="TFrom">The type of the source entity.</typeparam>
    /// <typeparam name="TTo">The type of the destination entity.</typeparam>
    /// <param name="from">The source entity.</param>
    /// <param name="to">The destination entity.</param>
    void Map<TFrom, TTo>(TFrom from, TTo to);

    /// <summary>
    /// Maps a entity.
    /// </summary>
    /// <typeparam name="TTo">The type of the destination entity.</typeparam>
    /// <param name="from">The source entity.</param>
    /// <returns>The destination entity.</returns>
    TTo Map<TTo>(object from);
}