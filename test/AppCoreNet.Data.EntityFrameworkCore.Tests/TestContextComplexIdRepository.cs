using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore;

public class TestContextComplexIdRepository
    : DbContextRepository<VersionId, EntityWithComplexId, TestContext, DbEntityWithComplexId>
{
    public TestContextComplexIdRepository(DbContextDataProvider<TestContext> provider, ILogger<TestContextComplexIdRepository> logger)
        : base(provider, logger)
    {
    }
}