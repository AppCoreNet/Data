// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Collections.Generic;

namespace AppCoreNet.Data.EntityFrameworkCore;

internal sealed class PagedResult<TResult> : IPagedResult<TResult>
{
    public long? TotalCount { get; }

    public IReadOnlyCollection<TResult> Items { get; }

    public PagedResult(IReadOnlyCollection<TResult> items, long? totalCount)
    {
        TotalCount = totalCount;
        Items = items;
    }
}