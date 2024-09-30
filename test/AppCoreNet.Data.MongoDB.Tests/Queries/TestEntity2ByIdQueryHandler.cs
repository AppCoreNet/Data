// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Data.Entities;
using AppCoreNet.Data.Queries;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB.Queries;

public class TestEntity2ByIdQueryHandler : MongoTestEntity2Repository.ScalarQueryHandler<TestEntity2ByIdQuery, TestEntity2>
{
    public TestEntity2ByIdQueryHandler(MongoDataProvider provider)
        : base(provider)
    {
    }

    protected override IFindFluent<DAO.TestEntity2, DAO.TestEntity2> ApplyQuery(
        IFindFluent<DAO.TestEntity2, DAO.TestEntity2> find,
        TestEntity2ByIdQuery query)
    {
        find.Filter = Builders<DAO.TestEntity2>.Filter.Eq(
            e => e.Id,
            new DAO.ComplexId { Id = query.Id.Id, Version = query.Id.Version });

        return find;
    }
}