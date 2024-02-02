using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

public class DbContextTestEntity2Repository
    : DbContextRepository<Entities.ComplexId, Entities.TestEntity2, TestContext, DAO.TestEntity2>, ITestEntity2Repository
{
    public DbContextTestEntity2Repository(
        DbContextDataProvider<TestContext> provider,
        ILogger<DbContextTestEntity2Repository> logger)
        : base(provider, logger)
    {
    }
}