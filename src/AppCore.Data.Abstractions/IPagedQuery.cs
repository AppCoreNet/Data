// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System.Collections.Generic;

namespace AppCore.Data;

/// <summary>
/// Represents a paged query.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IPagedQuery<TEntity, TResult> : IQuery<TEntity, IPagedResult<TResult>>
    where TEntity : IEntity
{
    /// <summary>
    /// Whether to query the total count.
    /// </summary>
    bool TotalCount { get; }

    /// <summary>
    /// Gets the result offset.
    /// </summary>
    long Offset { get; }

    /// <summary>
    /// Gets the result limit.
    /// </summary>
    int Limit { get; }
}