using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContextChangeTokenRepository
        : DbContextRepository<int, EntityWithChangeToken, TestContext, DbEntityWithChangeToken>
    {
        public TestContextChangeTokenRepository(
            IDbContextDataProvider<TestContext> provider,
            ITokenGenerator tokenGenerator,
            IEntityMapper entityMapper,
            ILogger logger)
            : base(provider, tokenGenerator, entityMapper, logger)
        {
        }
    }
}