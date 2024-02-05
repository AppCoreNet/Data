// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register the data services.
/// </summary>
public static class DataProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds the data services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">Delegate to configure the <see cref="IDataProviderBuilder"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> to chain the calls.</returns>
    public static IServiceCollection AddDataProvider(this IServiceCollection services, Action<IDataProviderBuilder>? configure = null)
    {
        Ensure.Arg.NotNull(services);

        services.TryAddSingleton(typeof(DataProviderLogger<>), typeof(DefaultDataProviderLogger<>));
        services.TryAddTransient<ITokenGenerator, TokenGenerator>();
        services.TryAddTransient<IDataProviderResolver, DataProviderResolver>();

        if (configure != null)
        {
            var builder = new DataProviderBuilder(services);
            configure(builder);
        }

        return services;
    }
}