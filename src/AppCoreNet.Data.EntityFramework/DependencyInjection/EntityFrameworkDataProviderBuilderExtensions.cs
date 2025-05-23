// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.EntityFramework; // Adjusted namespace
using AppCoreNet.Diagnostics;
using System.Data.Entity; // Added for DbContext
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register a <see cref="EntityFrameworkDataProvider{TDbContext}"/>.
/// </summary>
public static class EntityFrameworkDataProviderBuilderExtensions // Renamed class
{
    private const int DefaultPoolSize = 128; // This might be unused now as pooling was EF Core specific

    /// <summary>
    /// Registers a <see cref="System.Data.Entity.DbContext"/> data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="System.Data.Entity.DbContext"/> on your own (e.g. `services.AddScoped<MyDbContext>()`).
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkDataProviderBuilder<TDbContext> AddEntityFramework<TDbContext>( // Renamed method
        this IDataProviderBuilder builder,
        string name,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : System.Data.Entity.DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddOptions();
        builder.Services.AddLogging();

        builder.AddProvider<EntityFrameworkDataProvider<TDbContext>>( // Use new provider type
            name,
            providerLifetime,
            static (sp, name) =>
            {
                EntityFrameworkDataProviderServices<TDbContext> services = // Use new services type
                    EntityFrameworkDataProviderServices<TDbContext>.Create(name, sp);

                return new EntityFrameworkDataProvider<TDbContext>(name, services); // Instantiate new provider type
            });

        return new EntityFrameworkDataProviderBuilder<TDbContext>(name, builder.Services, providerLifetime); // Return new builder type
    }

    /// <summary>
    /// Registers a <see cref="System.Data.Entity.DbContext"/> default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="System.Data.Entity.DbContext"/> on your own (e.g. `services.AddScoped<MyDbContext>()`).
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkDataProviderBuilder<TDbContext> AddEntityFramework<TDbContext>( // Renamed method
        this IDataProviderBuilder builder,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : System.Data.Entity.DbContext
    {
        return builder.AddEntityFramework<TDbContext>(string.Empty, providerLifetime); // Call renamed method
    }

    // AddDbContext and AddDbContextPool methods are specific to EF Core and its DI extensions.
    // EF6 DbContexts are typically managed differently (e.g., direct instantiation or manual DI registration).
    // For this port, we will remove these EF Core specific extensions.
    // Users will be responsible for registering their TDbContext with the IServiceCollection if needed,
    // for example: services.AddScoped<MyDbContext>();
    // Or, if the DbContext has a parameterless constructor or takes a connection string name,
    // it can often be newed up directly where needed or via a factory.
}