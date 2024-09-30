// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Linq;
using AppCoreNet.Data.Entities;
using AppCoreNet.Data.Queries;

namespace AppCoreNet.Data.EntityFrameworkCore.Queries;

public class TestEntity2ByIdQueryHandler : DbContextTestEntity2Repository.ScalarQueryHandler<TestEntity2ByIdQuery, TestEntity2>
{
    public TestEntity2ByIdQueryHandler(DbContextDataProvider<TestContext> provider)
        : base(provider)
    {
    }

    protected override IQueryable<DAO.TestEntity2> ApplyQuery(IQueryable<DAO.TestEntity2> queryable, TestEntity2ByIdQuery query)
    {
        return queryable.Where(e => e.Id == query.Id.Id && e.Version == query.Id.Version);
    }

    protected override IQueryable<TestEntity2?> ApplyProjection(IQueryable<DAO.TestEntity2> queryable, TestEntity2ByIdQuery query)
    {
        return queryable.Select(
            e => new TestEntity2()
            {
                Id = new ComplexId
                {
                    Id = e.Id,
                    Version = e.Version,
                },
                ChangeToken = e.ChangeToken,
                Name = e.Name,
            });
    }
}