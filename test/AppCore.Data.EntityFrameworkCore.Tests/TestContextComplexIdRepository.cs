using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContextComplexIdRepository
        : DbContextRepository<VersionId, EntityWithComplexId, TestContext, DbEntityWithComplexId>
    {
        public TestContextComplexIdRepository(
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