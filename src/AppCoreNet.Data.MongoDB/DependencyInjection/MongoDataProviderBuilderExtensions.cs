// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data;
using AppCoreNet.Data.MongoDB;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register MongoDB data providers.
/// </summary>
public static class MongoDataProviderBuilderExtensions
{
    /// <summary>
    /// Registers a MongoDB data provider.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="configure">Optional delegate to configure the <see cref="MongoDataProviderOptions"/>.</param>
    /// <returns>A <see cref="MongoDataProviderBuilder"/> to further configure the data provider.</returns>
    public static MongoDataProviderBuilder AddMongoDb(
        this IDataProviderBuilder builder,
        string name,
        Action<MongoDataProviderOptions>? configure)
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        IServiceCollection services = builder.Services;

        services.AddOptions();
        services.AddLogging();

        if (configure != null)
        {
            services.Configure(name, configure);
        }

        services.TryAddKeyedSingleton<MongoClientProvider>(
            name,
            static (sp, name) =>
            {
                MongoDataProviderOptions options = GetOptions(sp, (string)name!);
                return new MongoClientProvider(options.ClientSettings);
            });

        services.TryAddKeyedSingleton<MongoTransactionManager>(
            name,
            static (sp, name) =>
            {
                var clientFactory = sp.GetRequiredKeyedService<MongoClientProvider>(name);
                IMongoClient client = clientFactory.GetClient();
                return new MongoTransactionManager(
                    client,
                    sp.GetRequiredService<DataProviderLogger<MongoDataProvider>>());
            });

        services.TryAddKeyedSingleton<MongoQueryHandlerFactory>(
            name,
            static (sp, name) =>
            {
                MongoDataProviderOptions options = GetOptions(sp, (string)name!);
                return new MongoQueryHandlerFactory(sp, options.QueryHandlerTypes);
            });

        builder.AddProvider<MongoDataProvider>(
            name,
            ServiceLifetime.Singleton,
            static (sp, name) =>
            {
                MongoDataProviderOptions options = GetOptions(sp, name);
                var clientProvider = sp.GetRequiredKeyedService<MongoClientProvider>(name);
                IMongoClient client = clientProvider.GetClient();
                IMongoDatabase database = client.GetDatabase(options.DatabaseName, options.DatabaseSettings);

                return new MongoDataProvider(
                    name,
                    database,
                    options.EntityMapperFactory(sp),
                    options.TokenGeneratorFactory(sp),
                    sp.GetRequiredKeyedService<MongoQueryHandlerFactory>(name),
                    sp.GetRequiredKeyedService<MongoTransactionManager>(name),
                    sp.GetRequiredService<DataProviderLogger<MongoDataProvider>>());
            });

        return new MongoDataProviderBuilder(name, services);

        static MongoDataProviderOptions GetOptions(IServiceProvider sp, string name)
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<MongoDataProviderOptions>>();
            return optionsMonitor.Get(name);
        }
    }

    /// <summary>
    /// Registers a default MongoDB data provider.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="configure">Optional delegate to configure the <see cref="MongoDataProviderOptions"/>.</param>
    /// <returns>A <see cref="MongoDataProviderBuilder"/> to further configure the data provider.</returns>
    public static MongoDataProviderBuilder AddMongoDb(
        this IDataProviderBuilder builder,
        Action<MongoDataProviderOptions>? configure)
    {
        return builder.AddMongoDb(string.Empty, configure);
    }
}