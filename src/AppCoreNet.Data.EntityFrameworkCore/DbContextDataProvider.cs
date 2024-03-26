// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Represents a Entity Framework Core data provider.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class DbContextDataProvider<TDbContext> : IDataProvider
    where TDbContext : DbContext
{
    private readonly string _name;
    private readonly DbContextDataProviderServices<TDbContext> _services;

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
    /// Gets the <see cref="DbContextQueryHandlerFactory{TDbContext}"/> of the data provider.
    /// </summary>
    public DbContextQueryHandlerFactory<TDbContext> QueryHandlerFactory => _services.QueryHandlerFactory;

    /// <summary>
    /// Gets the <see cref="DbContextTransactionManager{TDbContext}"/>.
    /// </summary>
    public DbContextTransactionManager<TDbContext> TransactionManager => _services.TransactionManager;

    ITransactionManager IDataProvider.TransactionManager => TransactionManager;

    internal DataProviderLogger<DbContextDataProvider<TDbContext>> Logger => _services.Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextDataProvider{TDbContext}"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="DbContextDataProviderServices{TDbContext}"/>.</param>
    public DbContextDataProvider(
        string name,
        DbContextDataProviderServices<TDbContext> services)
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
        #if NET462 || NETSTANDARD2_0
        CascadeTiming cascadeDeleteTiming = DbContext.ChangeTracker.CascadeDeleteTiming;
        CascadeTiming deleteOrphansTiming = DbContext.ChangeTracker.DeleteOrphansTiming;
        try
        {
            DbContext.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
            DbContext.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
            EntityEntry[] entries = DbContext.ChangeTracker.Entries().ToArray();
            foreach (EntityEntry entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }
        finally
        {
            DbContext.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming;
            DbContext.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming;
        }
        #else
        DbContext.ChangeTracker.Clear();
        #endif
    }
}
