// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.ComponentModel;
using AppCoreNet.Data;
using AppCoreNet.Data.MongoDB;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Represents the builder for Mongo DB data providers.
/// </summary>
public sealed class MongoDataProviderBuilder
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
    /// Initializes a new instance of the <see cref="MongoDataProviderBuilder"/> class.
    /// </summary>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public MongoDataProviderBuilder(string name, IServiceCollection services)
    {
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(services);

        Name = name;
        Services = services;
    }

    /// <summary>
    /// Adds the specified repository implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the repository.</typeparam>
    /// <returns>The <see cref="MongoDataProviderBuilder"/>.</returns>
    public MongoDataProviderBuilder AddRepository<TImplementation>()
        where TImplementation : class, IMongoRepository
    {
        return AddRepository<TImplementation, TImplementation>();
    }

    /// <summary>
    /// Adds the specified repository implementation.
    /// </summary>
    /// <typeparam name="TService">The type of the repository service.</typeparam>
    /// <typeparam name="TImplementation">The type of the repository implementation.</typeparam>
    /// <returns>The <see cref="MongoDataProviderBuilder"/>.</returns>
    public MongoDataProviderBuilder AddRepository<TService, TImplementation>()
        where TService : class
        where TImplementation : class, IMongoRepository, TService
    {
        Services.TryAddEnumerable(
            ServiceDescriptor.Transient<TService, TImplementation>(
                sp =>
                {
                    var resolver = sp.GetRequiredService<IDataProviderResolver>();
                    var provider = (MongoDataProvider)resolver.Resolve(Name);
                    return ActivatorUtilities.CreateInstance<TImplementation>(sp, provider);
                }));

        return this;
    }

    /// <summary>
    /// Adds the specified query handler implementation.
    /// </summary>
    /// <typeparam name="TQueryHandler">The type of the <see cref="MongoQueryHandler{TQuery,TEntity,TResult,TDbEntity}"/>.</typeparam>
    /// <returns>The <see cref="MongoDataProviderBuilder"/>.</returns>
    public MongoDataProviderBuilder AddQueryHandler<TQueryHandler>()
        where TQueryHandler : class, IMongoQueryHandler
    {
        Type queryHandlerType = typeof(TQueryHandler);

        Services.Configure<MongoDataProviderOptions>(
            Name,
            o => o.QueryHandlerTypes.Add(queryHandlerType));

        return this;
    }

    /// <summary>
    /// Registers a <see cref="ITokenGenerator"/> which generates concurrency tokens.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ITokenGenerator"/>.</typeparam>
    /// <returns>The <see cref="MongoDataProviderBuilder"/>.</returns>
    public MongoDataProviderBuilder AddTokenGenerator<T>()
        where T : class, ITokenGenerator
    {
        Services.Configure<MongoDataProviderOptions>(
            Name,
            o =>
                o.TokenGeneratorFactory = static sp =>
                    ActivatorUtilities.GetServiceOrCreateInstance<T>(sp));

        return this;
    }

    /// <summary>
    /// Registers a <see cref="IEntityMapper"/> which maps entities to database entities.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEntityMapper"/>.</typeparam>
    /// <returns>The <see cref="MongoDataProviderBuilder"/>.</returns>
    public MongoDataProviderBuilder AddEntityMapper<T>()
        where T : class, IEntityMapper
    {
        Services.Configure<MongoDataProviderOptions>(
            Name,
            o =>
                o.EntityMapperFactory = static sp =>
                    ActivatorUtilities.GetServiceOrCreateInstance<T>(sp));

        return this;
    }
}