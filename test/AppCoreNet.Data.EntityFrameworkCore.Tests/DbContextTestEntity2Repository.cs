// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextTestEntity2Repository
    : DbContextRepository<Entities.ComplexId, Entities.TestEntity2, TestContext, DAO.TestEntity2>, ITestEntity2Repository
{
    public DbContextTestEntity2Repository(DbContextDataProvider<TestContext> provider)
        : base(provider)
    {
    }
}