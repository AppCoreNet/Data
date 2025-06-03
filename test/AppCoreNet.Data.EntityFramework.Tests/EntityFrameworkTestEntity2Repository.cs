// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Data.EntityFramework.DAO;

namespace AppCoreNet.Data.EntityFramework;

public class EntityFrameworkTestEntity2Repository
    : EntityFrameworkRepository<Entities.ComplexId, Entities.TestEntity2, TestDbContext, DAO.TestDao2>, ITestEntity2Repository
{
    public EntityFrameworkTestEntity2Repository(EntityFrameworkDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }
}