using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class TestContextSimpleIdRepository
    : DbContextRepository<int, EntityWithSimpleId, TestContext, DbEntityWithSimpleId>
{
    public TestContextSimpleIdRepository(DbContextDataProvider<TestContext> provider, ILogger<TestContextSimpleIdRepository> logger)
        : base(provider, logger)
    {
    }
}