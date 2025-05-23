// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.ComponentModel;
using AppCoreNet.Data;
using AppCoreNet.Data.EntityFramework; // Adjusted namespace
using AppCoreNet.Diagnostics;
using System.Data.Entity; // Added for DbContext
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Represents the builder for Entity Framework data providers.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public sealed class EntityFrameworkDataProviderBuilder<TDbContext>
    where TDbContext : System.Data.Entity.DbContext // Adjusted constraint
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
    /// Initializes a new instance of the <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="providerLifetime">The lifetime of the <see cref="IDataProvider"/>.</param>
    public EntityFrameworkDataProviderBuilder(string name, IServiceCollection services, ServiceLifetime providerLifetime)
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
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public EntityFrameworkDataProviderBuilder<TDbContext> AddRepository<TImplementation>()
        where TImplementation : class, IEntityFrameworkRepository<TDbContext>
    {
        return AddRepository<TImplementation, TImplementation>();
    }

    /// <summary>
    /// Adds the specified repository implementation.
    /// </summary>
    /// <typeparam name="TService">The type of the repository service.</typeparam>
    /// <typeparam name="TImplementation">The type of the repository implementation.</typeparam>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public EntityFrameworkDataProviderBuilder<TDbContext> AddRepository<TService, TImplementation>()
        where TService : class
        where TImplementation : class, IEntityFrameworkRepository<TDbContext>, TService
    {
        Services.TryAddEnumerable(
            ServiceDescriptor.Describe(
                typeof(TService),
                new Func<IServiceProvider, TImplementation>(
                    sp =>
                    {
                        var provider =
                            (EntityFrameworkDataProvider<TDbContext>)sp.GetRequiredService<IDataProviderResolver>()
                                                                 .Resolve(Name);

                        return ActivatorUtilities.CreateInstance<TImplementation>(sp, provider);
                    }),
                ProviderLifetime));

        return this;
    }

    /// <summary>
    /// Adds the specified query handler implementation.
    /// </summary>
    /// <typeparam name="TQueryHandler">The type of the <see cref="EntityFrameworkQueryHandler{TQuery,TEntity,TResult,TDbContext,TDbEntity}"/>.</typeparam>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public EntityFrameworkDataProviderBuilder<TDbContext> AddQueryHandler<TQueryHandler>()
        where TQueryHandler : class, IEntityFrameworkQueryHandler<TDbContext>
    {
        Type queryHandlerType = typeof(TQueryHandler);

        Services.Configure<EntityFrameworkDataProviderOptions>(
            Name,
            o => o.QueryHandlerTypes.Add(queryHandlerType));

        return this;
    }

    /// <summary>
    /// Registers a <see cref="ITokenGenerator"/> which generates concurrency tokens.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ITokenGenerator"/>.</typeparam>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public EntityFrameworkDataProviderBuilder<TDbContext> AddTokenGenerator<T>()
        where T : class, ITokenGenerator
    {
        Services.Configure<EntityFrameworkDataProviderOptions>(
            Name,
            o => o.TokenGeneratorType = typeof(T));

        return this;
    }

    /// <summary>
    /// Registers a <see cref="IEntityMapper"/> which maps entities to database entities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEntityMapper"/>.</typeparam>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public EntityFrameworkDataProviderBuilder<TDbContext> AddEntityMapper<T>()
        where T : class, IEntityMapper
    {
        Services.Configure<EntityFrameworkDataProviderOptions>(
            Name,
            o => o.EntityMapperType = typeof(T));

        return this;
    }
}