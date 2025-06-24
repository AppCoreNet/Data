// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Linq;
using AppCoreNet.Data.Entities;
using AppCoreNet.Data.EntityFramework.DAO;
using AppCoreNet.Data.Queries;

namespace AppCoreNet.Data.EntityFramework.Queries;

public class TestEntityByIdQueryHandler : DbContextTestEntityRepository.ScalarQueryHandler<TestEntityByIdQuery, TestEntity>
{
    public TestEntityByIdQueryHandler(DbContextDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }

    protected override IQueryable<DAO.TestDao> ApplyQuery(IQueryable<DAO.TestDao> queryable, TestEntityByIdQuery query)
    {
        return queryable.Where(e => e.Id == query.Id);
    }

    protected override IQueryable<TestEntity?> ApplyProjection(IQueryable<DAO.TestDao> queryable, TestEntityByIdQuery query)
    {
        return queryable.Select(e => new TestEntity() { Id = e.Id, ChangeToken = e.ChangeToken, Name = e.Name });
    }
}