using Microsoft.Extensions.Logging;

namespace AppCore.Data.EntityFrameworkCore;

public class TestContextChangeTokenExRepository
    : DbContextRepository<int, EntityWithChangeTokenEx, TestContext, DbEntityWithChangeToken>
{
    public TestContextChangeTokenExRepository(
        IDbContextDataProvider<TestContext> provider,
        IDbContextQueryHandlerProvider<TestContext> queryHandlerProvider,
        ITokenGenerator tokenGenerator,
        IEntityMapper entityMapper,
        ILogger logger)
        : base(provider, queryHandlerProvider, tokenGenerator, entityMapper, logger)
    {
    }
}