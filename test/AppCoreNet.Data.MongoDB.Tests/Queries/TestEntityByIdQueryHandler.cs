// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Data.Entities;
using AppCoreNet.Data.Queries;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB.Queries;

public class TestEntityByIdQueryHandler : MongoTestEntityRepository.ScalarQueryHandler<TestEntityByIdQuery, TestEntity>
{
    public TestEntityByIdQueryHandler(MongoDataProvider provider)
        : base(provider)
    {
    }

    protected override IFindFluent<DAO.TestEntity, DAO.TestEntity> ApplyQuery(
        IFindFluent<DAO.TestEntity, DAO.TestEntity> find,
        TestEntityByIdQuery query)
    {
        find.Filter = Builders<DAO.TestEntity>.Filter.Eq(e => e.Id, query.Id);
        return find;
    }
}