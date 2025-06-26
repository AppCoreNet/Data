// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Represents a Entity Framework Core data provider.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class DbContextDataProvider<TDbContext> : IDataProvider
    where TDbContext : DbContext
{
    /// <inheritdoc />
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="DbContext"/> of the data provider.
    /// </summary>
    /// <value>The <see cref="DbContext"/>.</value>
    public TDbContext DbContext { get; }

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/> of the data provider.
    /// </summary>
    public IEntityMapper EntityMapper { get; }

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/> of the data provider.
    /// </summary>
    public ITokenGenerator TokenGenerator { get; }

    /// <summary>
    /// Gets the <see cref="DbContextQueryHandlerFactory{TDbContext}"/> of the data provider.
    /// </summary>
    public DbContextQueryHandlerFactory<TDbContext> QueryHandlerFactory { get; }

    /// <summary>
    /// Gets the <see cref="DbContextTransactionManager{TDbContext}"/>.
    /// </summary>
    public DbContextTransactionManager<TDbContext> TransactionManager { get; }

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    internal DataProviderLogger<DbContextDataProvider<TDbContext>> Logger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextDataProvider{TDbContext}"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="dbContext">The <see cref="DbContext"/>.</param>
    /// <param name="entityMapper">The <see cref="IEntityMapper"/>.</param>
    /// <param name="tokenGenerator">The <see cref="ITokenGenerator"/>.</param>
    /// <param name="queryHandlerFactory">The <see cref="DbContextQueryHandlerFactory{TDbContext}"/>.</param>
    /// <param name="transactionManager">The <see cref="DbContextTransactionManager{TDbContext}"/>.</param>
    /// <param name="logger">The <see cref="DataProviderLogger{T}"/>.</param>
    public DbContextDataProvider(
        string name,
        TDbContext dbContext,
        IEntityMapper entityMapper,
        ITokenGenerator tokenGenerator,
        DbContextQueryHandlerFactory<TDbContext> queryHandlerFactory,
        DbContextTransactionManager<TDbContext> transactionManager,
        DataProviderLogger<DbContextDataProvider<TDbContext>> logger)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(dbContext);
        Ensure.Arg.NotNull(entityMapper);
        Ensure.Arg.NotNull(tokenGenerator);
        Ensure.Arg.NotNull(queryHandlerFactory);
        Ensure.Arg.NotNull(transactionManager);
        Ensure.Arg.NotNull(logger);

        Name = name;
        DbContext = dbContext;
        EntityMapper = entityMapper;
        TokenGenerator = tokenGenerator;
        QueryHandlerFactory = queryHandlerFactory;
        TransactionManager = transactionManager;
        Logger = logger;
    }

    /// <summary>
    /// Saves changes made to the <see cref="DbContext"/>.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await DbContext.SaveChangesAsync(cancellationToken)
                           .ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException error)
        {
            throw new EntityConcurrencyException(error);
        }
        catch (DbUpdateException error)
        {
            throw new EntityUpdateException(error);
        }

        DbContext.ChangeTracker.Clear();
    }
}