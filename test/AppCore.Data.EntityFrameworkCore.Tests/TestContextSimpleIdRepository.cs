using AppCore.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContextSimpleIdRepository
        : DbContextRepository<int, EntityWithSimpleId, TestContext, DbEntityWithSimpleId>
    {
        public TestContextSimpleIdRepository(
            IDbContextDataProvider<TestContext> provider,
            ITokenGenerator tokenGenerator,
            IEntityMapper entityMapper,
            ILogger logger)
            : base(provider, tokenGenerator, entityMapper, logger)
        {
        }
    }
}