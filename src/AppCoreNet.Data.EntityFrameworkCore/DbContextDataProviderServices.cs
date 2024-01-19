using Microsoft.EntityFrameworkCore;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Provides services for <see cref="DbContext"/> based data provider.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class DbContextDataProviderServices<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the <see cref="DbContext"/>.
    /// </summary>
    public TDbContext DbContext { get; }

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/>.
    /// </summary>
    public IEntityMapper EntityMapper { get; }

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/>.
    /// </summary>
    public ITokenGenerator TokenGenerator { get; }

    /// <summary>
    /// Gets the <see cref="DbContextQueryHandlerFactory{TDbContext}"/>.
    /// </summary>
    public DbContextQueryHandlerFactory<TDbContext> QueryHandlerFactory { get; }

    /// <summary>
    /// Gets the <see cref="DbContextTransactionManager"/>.
    /// </summary>
    public DbContextTransactionManager TransactionManager { get; }

    internal DbContextDataProviderServices(
        TDbContext dbContext,
        IEntityMapper entityMapper,
        ITokenGenerator tokenGenerator,
        DbContextQueryHandlerFactory<TDbContext> queryHandlerFactory,
        DbContextTransactionManager transactionManager)
    {
        DbContext = dbContext;
        EntityMapper = entityMapper;
        TokenGenerator = tokenGenerator;
        QueryHandlerFactory = queryHandlerFactory;
        TransactionManager = transactionManager;
    }
}