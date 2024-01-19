// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.ComponentModel;
using AppCoreNet.Data;
using AppCoreNet.Data.EntityFrameworkCore;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Represents the builder for Entity Framework Core data providers.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class DbContextDataProviderBuilder<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// Gets the name of the provider.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string Name { get; }

    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public IServiceCollection Services { get; }

    /// <summary>
    /// Gets the <see cref="ProviderLifetime"/> of the provider.
    /// </summary>
    public ServiceLifetime ProviderLifetime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextDataProviderBuilder{TDbContext}"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="providerLifetime">The lifetime of the <see cref="IDataProvider"/>.</param>
    public DbContextDataProviderBuilder(string name, IServiceCollection services, ServiceLifetime providerLifetime)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(services);

        Name = name;
        Services = services;
        ProviderLifetime = providerLifetime;
    }

    /// <summary>
    /// Adds the specified repository implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the repository.</typeparam>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public DbContextDataProviderBuilder<TDbContext> AddRepository<TImplementation>()
        where TImplementation : class, IDbContextRepository<TDbContext>
    {
        return AddRepository<TImplementation, TImplementation>();
    }

    /// <summary>
    /// Adds the specified repository implementation.
    /// </summary>
    /// <typeparam name="TService">The type of the repository service.</typeparam>
    /// <typeparam name="TImplementation">The type of the repository implementation.</typeparam>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public DbContextDataProviderBuilder<TDbContext> AddRepository<TService, TImplementation>()
        where TService : class
        where TImplementation : class, IDbContextRepository<TDbContext>, TService
    {
        Services.TryAdd(
            ServiceDescriptor.Describe(
                typeof(TService),
                sp =>
                {
                    var provider = (DbContextDataProvider<TDbContext>)
                        sp.GetRequiredService<IDataProviderResolver>()
                          .Resolve(Name);

                    return ActivatorUtilities.CreateInstance<TImplementation>(sp, provider);
                },
                ProviderLifetime));

        return this;
    }

    /// <summary>
    /// Adds the specified query handler implementation.
    /// </summary>
    /// <typeparam name="TQueryHandler">The type of the <see cref="DbContextQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/>.</typeparam>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public DbContextDataProviderBuilder<TDbContext> AddQueryHandler<TQueryHandler>()
        where TQueryHandler : class, IDbContextQueryHandler<TDbContext>
    {
        Type queryHandlerType = typeof(TQueryHandler);

        Services.Configure<DbContextDataProviderOptions>(
            Name,
            o => o.QueryHandlerTypes.Add(queryHandlerType));

        return this;
    }

    /// <summary>
    /// Registers a <see cref="ITokenGenerator"/> which generates concurrency tokens.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ITokenGenerator"/>.</typeparam>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public DbContextDataProviderBuilder<TDbContext> AddTokenGenerator<T>()
        where T : class, ITokenGenerator
    {
        Services.Configure<DbContextDataProviderOptions>(
            Name,
            o => o.TokenGeneratorType = typeof(T));

        return this;
    }

    /// <summary>
    /// Registers a <see cref="IEntityMapper"/> which maps entities to database entities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEntityMapper"/>.</typeparam>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public DbContextDataProviderBuilder<TDbContext> AddEntityMapper<T>()
        where T : class, IEntityMapper
    {
        Services.Configure<DbContextDataProviderOptions>(
            Name,
            o => o.EntityMapperType = typeof(T));

        return this;
    }
}