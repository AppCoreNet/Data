using System;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextTestEntityRepository
    : DbContextRepository<Guid, Entities.TestEntity, TestContext, DAO.TestEntity>, ITestEntityRepository
{
    public DbContextTestEntityRepository(
        DbContextDataProvider<TestContext> provider,
        ILogger<DbContextTestEntityRepository> logger)
        : base(provider, logger)
    {
    }
}