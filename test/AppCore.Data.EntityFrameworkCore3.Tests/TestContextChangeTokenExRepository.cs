using AppCore.Logging;

namespace AppCore.Data.EntityFrameworkCore
{
    public class TestContextChangeTokenExRepository
        : DbContextRepository<int, EntityWithChangeTokenEx, TestContext, DbEntityWithChangeToken>
    {
        public TestContextChangeTokenExRepository(
            IDbContextDataProvider<TestContext> provider,
            ITokenGenerator tokenGenerator,
            IEntityMapper entityMapper,
            ILogger logger)
            : base(provider, tokenGenerator, entityMapper, logger)
        {
        }
    }
}