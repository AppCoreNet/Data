// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data.MongoDB;

public class MongoTestEntityRepository : MongoRepository<Guid, Entities.TestEntity, DAO.TestEntity>, ITestEntityRepository
{
    public MongoTestEntityRepository(MongoDataProvider provider)
        : base(provider)
    {
    }
}