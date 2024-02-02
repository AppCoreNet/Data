using System.Linq;
using AppCoreNet.Data.Entities;
using AppCoreNet.Data.Queries;

namespace AppCoreNet.Data.EntityFrameworkCore.Queries;

public class TestEntityByIdQueryHandler : DbContextTestEntityRepository.ScalarQueryHandler<TestEntityByIdQuery, TestEntity>
{
    public TestEntityByIdQueryHandler(DbContextDataProvider<TestContext> provider)
        : base(provider)
    {
    }

    protected override IQueryable<DAO.TestEntity> ApplyQuery(IQueryable<DAO.TestEntity> queryable, TestEntityByIdQuery query)
    {
        return queryable.Where(e => e.Id == query.Id);
    }

    protected override IQueryable<TestEntity?> ApplyProjection(IQueryable<DAO.TestEntity> queryable, TestEntityByIdQuery query)
    {
        return queryable.Select(e => new TestEntity() { Id = e.Id, ChangeToken = e.ChangeToken, Name = e.Name });
    }
}