// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class TestContextChangeTokenExRepository
    : DbContextRepository<int, EntityWithChangeTokenEx, TestContext, DbEntityWithChangeToken>
{
    public TestContextChangeTokenExRepository(DbContextDataProvider<TestContext> provider, ILogger<TestContextChangeTokenExRepository> logger)
        : base(provider, logger)
    {
    }
}