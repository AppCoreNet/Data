using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContextChangeTokenRepository
        : DbContextRepository<int, EntityWithChangeToken, TestContext, DbEntityWithChangeToken>
    {
        public TestContextChangeTokenRepository(
            IDbContextDataProvider<TestContext> provider,
            IDbContextQueryHandlerProvider queryHandlerProvider,
            ITokenGenerator tokenGenerator,
            IEntityMapper entityMapper,
            ILogger logger)
            : base(provider, queryHandlerProvider, tokenGenerator, entityMapper, logger)
        {
        }
    }
}