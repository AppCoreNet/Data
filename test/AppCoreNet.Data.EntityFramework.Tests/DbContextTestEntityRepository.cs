// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.EntityFramework.DAO;

namespace AppCoreNet.Data.EntityFramework;

public class DbContextTestEntityRepository
    : DbContextRepository<Guid, Entities.TestEntity, TestDbContext, DAO.TestDao>, ITestEntityRepository
{
    public DbContextTestEntityRepository(DbContextDataProvider<TestDbContext> provider)
        : base(provider)
    {
    }
}