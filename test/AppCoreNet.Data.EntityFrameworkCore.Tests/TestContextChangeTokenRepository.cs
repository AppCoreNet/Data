// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class TestContextChangeTokenRepository
    : DbContextRepository<int, EntityWithChangeToken, TestContext, DbEntityWithChangeToken>
{
    public TestContextChangeTokenRepository(DbContextDataProvider<TestContext> provider, ILogger<TestContextChangeTokenRepository> logger)
        : base(provider, logger)
    {
    }
}