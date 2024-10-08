﻿// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextTestEntityRepository
    : DbContextRepository<Guid, Entities.TestEntity, TestContext, DAO.TestEntity>, ITestEntityRepository
{
    public DbContextTestEntityRepository(DbContextDataProvider<TestContext> provider)
        : base(provider)
    {
    }
}