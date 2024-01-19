// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register the data services.
/// </summary>
public static class DataProvidersServiceCollectionExtensions
{
    /// <summary>
    /// Adds the data services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configure">Delegate to configure the <see cref="IDataProvidersBuilder"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddDataProviders(this IServiceCollection services, Action<IDataProvidersBuilder>? configure = null)
    {
        Ensure.Arg.NotNull(services);

        services.TryAddTransient<ITokenGenerator, TokenGenerator>();
        services.TryAddTransient<IDataProviderResolver, DataProviderResolver>();

        if (configure != null)
        {
            var builder = new DataProvidersBuilder(services);
            configure(builder);
        }

        return services;
    }
}