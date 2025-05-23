// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Data.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// Provides services for <see cref="DbContext"/> based data provider.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class EntityFrameworkDataProviderServices<TDbContext>
    where TDbContext : System.Data.Entity.DbContext
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
    /// Gets the <see cref="EntityFrameworkQueryHandlerFactory{TDbContext}"/>.
    /// </summary>
    public EntityFrameworkQueryHandlerFactory<TDbContext> QueryHandlerFactory { get; }

    /// <summary>
    /// Gets the <see cref="EntityFrameworkTransactionManager{TDbContext}"/>.
    /// </summary>
    public EntityFrameworkTransactionManager<TDbContext> TransactionManager { get; }

    /// <summary>
    /// Gets the <see cref="ILogger"/>.
    /// </summary>
    public DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> Logger { get; }

    internal EntityFrameworkDataProviderServices(
        TDbContext dbContext,
        IEntityMapper entityMapper,
        ITokenGenerator tokenGenerator,
        EntityFrameworkQueryHandlerFactory<TDbContext> queryHandlerFactory,
        EntityFrameworkTransactionManager<TDbContext> transactionManager,
        DataProviderLogger<EntityFrameworkDataProvider<TDbContext>> logger)
    {
        DbContext = dbContext;
        EntityMapper = entityMapper;
        TokenGenerator = tokenGenerator;
        QueryHandlerFactory = queryHandlerFactory;
        TransactionManager = transactionManager;
        Logger = logger;
    }

    private static T GetOrCreateInstance<T>(IServiceProvider serviceProvider, Type? type)
        where T : notnull
    {
        return type != null
            ? (T)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type)
            : serviceProvider.GetRequiredService<T>();
    }

    internal static EntityFrameworkDataProviderServices<TDbContext> Create(string name, IServiceProvider serviceProvider)
    {
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<EntityFrameworkDataProviderOptions>>();
        EntityFrameworkDataProviderOptions options = optionsMonitor.Get(name);

        var entityMapper = GetOrCreateInstance<IEntityMapper>(serviceProvider, options.EntityMapperType);
        var tokenGenerator = GetOrCreateInstance<ITokenGenerator>(serviceProvider, options.TokenGeneratorType);
        var logger = serviceProvider.GetRequiredService<DataProviderLogger<EntityFrameworkDataProvider<TDbContext>>>();

        var dbContext = serviceProvider.GetRequiredService<TDbContext>();
        var queryHandlerFactory = new EntityFrameworkQueryHandlerFactory<TDbContext>(serviceProvider, options.QueryHandlerTypes);
        var transactionManager = new EntityFrameworkTransactionManager<TDbContext>(dbContext, logger);

        return new EntityFrameworkDataProviderServices<TDbContext>(
            dbContext,
            entityMapper,
            tokenGenerator,
            queryHandlerFactory,
            transactionManager,
            logger);
    }
}