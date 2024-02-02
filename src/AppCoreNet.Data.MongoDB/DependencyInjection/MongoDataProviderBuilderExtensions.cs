// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.MongoDB;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

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
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="configure">Optional delegate to configure the <see cref="MongoDataProviderOptions"/>.</param>
    /// <returns>A <see cref="MongoDataProviderBuilder"/> to further configure the data provider.</returns>
    public static MongoDataProviderBuilder AddMongoDB(
        this IDataProvidersBuilder builder,
        string name,
        Action<MongoDataProviderOptions>? configure)
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddOptions();

        // TODO: builder.Services.AddLogging();
        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        builder.AddProvider<MongoDataProvider>(
            name,
            ServiceLifetime.Singleton,
            static (sp, name) =>
            {
                var services = MongoDataProviderServices.Create(name, sp);
                return new MongoDataProvider(name, services);
            });

        return new MongoDataProviderBuilder(name, builder.Services);
    }
}