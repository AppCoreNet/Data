using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContextSimpleIdRepository
        : DbContextRepository<int, EntityWithSimpleId, TestContext, DbEntityWithSimpleId>
    {
        public TestContextSimpleIdRepository(
            IDbContextDataProvider<TestContext> provider,
            IDbContextQueryHandlerProvider<TestContext> queryHandlerProvider,
            ITokenGenerator tokenGenerator,
            IEntityMapper entityMapper,
            ILogger logger)
            : base(provider, queryHandlerProvider, tokenGenerator, entityMapper, logger)
        {
        }
    }
}