// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace AppCoreNet.Data.MongoDB;

public class MongoTestEntity2Repository
    : MongoRepository<Entities.ComplexId, Entities.TestEntity2, DAO.TestEntity2>, ITestEntity2Repository
{
    public MongoTestEntity2Repository(MongoDataProvider provider)
        : base(provider)
    {
    }

    /*
    protected override BsonValue GetPrimaryKey(Entities.ComplexId id)
    {
        return new BsonDocument()
        {
            { "_id", GuidSerializer.StandardInstance.ToBsonValue(id.Id) },
            { "Version", id.Version },
        };
    }
    */
}