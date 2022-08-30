// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System.Collections.Generic;

namespace AppCore.Data.EntityFrameworkCore;

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