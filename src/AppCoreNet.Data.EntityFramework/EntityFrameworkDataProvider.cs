// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Represents a Entity Framework data provider.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class EntityFrameworkDataProvider<TDbContext> : IDataProvider
    where TDbContext : System.Data.Entity.DbContext
{
    private readonly string _name;
    private readonly EntityFrameworkDataProviderServices<TDbContext> _services;

    /// <inheritdoc />
    public string Name => _name;

    /// <summary>
    /// Gets the <see cref="DbContext"/> of the data provider.
    /// </summary>
    /// <value>The <see cref="DbContext"/>.</value>
    public TDbContext DbContext => _services.DbContext;

    /// <summary>
    /// Gets the <see cref="IEntityMapper"/> of the data provider.
    /// </summary>
    public IEntityMapper EntityMapper => _services.EntityMapper;

    /// <summary>
    /// Gets the <see cref="ITokenGenerator"/> of the data provider.
    /// </summary>
    public ITokenGenerator TokenGenerator => _services.TokenGenerator;

    /// <summary>
    /// Gets the <see cref="EntityFrameworkQueryHandlerFactory{TDbContext}"/> of the data provider.
    /// </summary>
    public EntityFrameworkQueryHandlerFactory<TDbContext> QueryHandlerFactory => _services.QueryHandlerFactory;

    /// <summary>
    /// Gets the <see cref="EntityFrameworkTransactionManager{TDbContext}"/>.
    /// </summary>
    public EntityFrameworkTransactionManager<TDbContext> TransactionManager => _services.TransactionManager;

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    internal DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> Logger => _services.Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityFrameworkDataProvider{TDbContext}"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="EntityFrameworkDataProviderServices{TDbContext}"/>.</param>
    public EntityFrameworkDataProvider(
        string name,
        EntityFrameworkDataProviderServices<TDbContext> services)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(services);

        _name = name;
        _services = services;
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
            // EF6 SaveChangesAsync returns Task<int>
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

        // detach all entities after saving changes
        foreach (var entry in DbContext.ChangeTracker.Entries())
        {
            if (entry.Entity != null) // Add null check for safety
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
