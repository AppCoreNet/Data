// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Collections.Generic;

namespace AppCoreNet.Data;

/// <summary>
/// Represents the result of a paged query.
/// </summary>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IPagedResult<out TResult>
{
    /// <summary>
    /// Gets the total count of items.
    /// </summary>
    long? TotalCount { get; }

    /// <summary>
    /// Gets the result items.
    /// </summary>
    IReadOnlyCollection<TResult> Items { get; }
}