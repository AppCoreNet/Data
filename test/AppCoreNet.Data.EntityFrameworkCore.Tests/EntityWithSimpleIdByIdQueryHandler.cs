// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class EntityWithSimpleIdByIdQueryHandler
    : TestContextSimpleIdRepository.ScalarQueryHandler<EntityWithSimpleIdByIdQuery, EntityWithSimpleId?>
{
    public static List<EntityWithSimpleIdByIdQuery> ExecutedQueries { get; } = new ();

    public EntityWithSimpleIdByIdQueryHandler(DbContextDataProvider<TestContext> provider)
        : base(provider)
    {
    }

    public override Task<EntityWithSimpleId?> ExecuteAsync(
        EntityWithSimpleIdByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ExecutedQueries.Add(query);
        return base.ExecuteAsync(query, cancellationToken);
    }

    protected override IQueryable<DbEntityWithSimpleId> ApplyQuery(
        IQueryable<DbEntityWithSimpleId> queryable,
        EntityWithSimpleIdByIdQuery query)
    {
        return queryable;
    }

    protected override IQueryable<EntityWithSimpleId?> ApplyProjection(
        IQueryable<DbEntityWithSimpleId> queryable,
        EntityWithSimpleIdByIdQuery query)
    {
        return queryable.Select(x => new EntityWithSimpleId { Id = x.Id, Value = x.Value });
    }
}