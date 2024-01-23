// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

    private static T GetOrCreateInstance<T>(IServiceProvider serviceProvider, Type? type)
        where T : notnull
    {
        return type != null
            ? (T)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type)
            : serviceProvider.GetRequiredService<T>();
    }

    internal static DbContextDataProviderServices<TDbContext> Create(string name, IServiceProvider serviceProvider)
    {
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<DbContextDataProviderOptions>>();
        DbContextDataProviderOptions options = optionsMonitor.Get(name);

        var entityMapper = GetOrCreateInstance<IEntityMapper>(serviceProvider, options.EntityMapperType);
        var tokenGenerator = GetOrCreateInstance<ITokenGenerator>(serviceProvider, options.TokenGeneratorType);

        var dbContext = serviceProvider.GetRequiredService<TDbContext>();
        var queryHandlerFactory = new DbContextQueryHandlerFactory<TDbContext>(serviceProvider, options.QueryHandlerTypes);
        var transactionManager = new DbContextTransactionManager(
            dbContext,
            serviceProvider.GetRequiredService<ILogger<DbContextTransactionManager>>());

        return new DbContextDataProviderServices<TDbContext>(
            dbContext,
            entityMapper,
            tokenGenerator,
            queryHandlerFactory,
            transactionManager);
    }
}