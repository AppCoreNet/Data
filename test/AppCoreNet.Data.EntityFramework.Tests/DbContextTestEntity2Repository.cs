// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Data.EntityFramework.DAO;

namespace AppCoreNet.Data.EntityFramework;

public class DbContextTestEntity2Repository
    : DbContextRepository<Entities.ComplexId, Entities.TestEntity2, TestDbContext, DAO.TestDao2>, ITestEntity2Repository
{
    public DbContextTestEntity2Repository(DbContextDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }
}