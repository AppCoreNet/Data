// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Linq;
using AppCoreNet.Data.Entities;
using AppCoreNet.Data.EntityFramework.DAO;
using AppCoreNet.Data.Queries;

namespace AppCoreNet.Data.EntityFramework.Queries;

public class TestEntity2ByIdQueryHandler : EntityFrameworkTestEntity2Repository.ScalarQueryHandler<TestEntity2ByIdQuery, TestEntity2>
{
    public TestEntity2ByIdQueryHandler(EntityFrameworkDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }

    protected override IQueryable<DAO.TestDao2> ApplyQuery(IQueryable<DAO.TestDao2> queryable, TestEntity2ByIdQuery query)
    {
        return queryable.Where(e => e.Id == query.Id.Id && e.Version == query.Id.Version);
    }

    protected override IQueryable<TestEntity2?> ApplyProjection(IQueryable<DAO.TestDao2> queryable, TestEntity2ByIdQuery query)
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