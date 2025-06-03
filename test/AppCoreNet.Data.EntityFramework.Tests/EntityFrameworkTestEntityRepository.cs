// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.EntityFramework.DAO;

namespace AppCoreNet.Data.EntityFramework;

public class EntityFrameworkTestEntityRepository
    : EntityFrameworkRepository<Guid, Entities.TestEntity, TestDbContext, DAO.TestDao>, ITestEntityRepository
{
    public EntityFrameworkTestEntityRepository(EntityFrameworkDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }
}