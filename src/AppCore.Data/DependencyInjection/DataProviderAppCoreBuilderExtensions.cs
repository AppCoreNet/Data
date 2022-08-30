// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register the data services.
/// </summary>
public static class DataProviderAppCoreBuilderExtensions
{
    /// <summary>
    /// Adds the data services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IAppCoreBuilder"/>.</param>
    /// <param name="configure">Delegate to configure the <see cref="IDataProviderBuilder"/>.</param>
    /// <returns>The <see cref="IAppCoreBuilder"/>.</returns>
    public static IAppCoreBuilder AddDataProvider(this IAppCoreBuilder builder, Action<IDataProviderBuilder>? configure = null)
    {
        Ensure.Arg.NotNull(builder);

        IServiceCollection services = builder.Services;

        services.TryAddSingleton<ITokenGenerator, TokenGenerator>();
        services.TryAddEnumerable(new []
        {
            ServiceDescriptor.Scoped(typeof(IDataProvider<>), typeof(DataProvider<>)),
            ServiceDescriptor.Scoped(typeof(ITransactionManager<>), typeof(TransactionManager<>))
        });

        configure?.Invoke(new DataProviderBuilder(services));

        return builder;
    }
}