// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Linq;
using AppCoreNet.Data;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IDataProviderBuilder"/>.
/// </summary>
public static class DataProviderBuilderExtensions
{
    /// <summary>
    /// Registers a data provider with a factory.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="lifetime">The lifetime of the data provider service.</param>
    /// <param name="factory">The factory method that creates the data provider.</param>
    /// <typeparam name="T">The type of the data provider.</typeparam>
    /// <returns>The <see cref="IDataProviderBuilder"/> to allow chaining.</returns>
    public static IDataProviderBuilder AddProvider<T>(
        this IDataProviderBuilder builder,
        string name,
        ServiceLifetime lifetime,
        Func<IServiceProvider, string, T> factory)
        where T : class, IDataProvider
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(factory);

        IServiceCollection services = builder.Services;

        // check whether a data provider with the same name and different type is already registered
        if (services.Any(sd => typeof(IDataProvider).IsAssignableFrom(sd.ServiceType)
                               && sd.IsKeyedService
                               && sd.ServiceType != typeof(IDataProvider)
                               && sd.ServiceType != typeof(T)
                               && (string?)sd.ServiceKey == name))
        {
            throw new InvalidOperationException(
                $"Data provider with name '{name}' is already registered with type '{typeof(T).GetDisplayName()}'.");
        }

        // check whether a data provider with the same name and type is already registered
        if (services.Any(sd => sd.IsKeyedService
                               && sd.ServiceType == typeof(T)
                               && (string?)sd.ServiceKey! == name))
        {
            return builder;
        }

        services.Add(
            ServiceDescriptor.DescribeKeyed(
                typeof(T),
                name,
                (sp, key) => factory(sp, (string)key!),
                lifetime));

        services.AddKeyedTransient<IDataProvider>(
            name,
            (sp, key) => sp.GetRequiredKeyedService<T>(key));

        services.AddTransient<T>(sp => sp.GetRequiredKeyedService<T>(name));
        services.AddTransient<IDataProvider>(sp => sp.GetRequiredKeyedService<T>(name));

        return builder;
    }

    /// <summary>
    /// Registers a data provider.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="lifetime">The lifetime of the data provider service.</param>
    /// <typeparam name="T">The type of the data provider.</typeparam>
    /// <returns>The <see cref="IDataProviderBuilder"/> to allow chaining.</returns>
    public static IDataProviderBuilder AddProvider<T>(
        this IDataProviderBuilder builder,
        string name,
        ServiceLifetime lifetime)
        where T : class, IDataProvider
    {
        return builder.AddProvider<T>(
            name,
            lifetime,
            static (sp, n) => ActivatorUtilities.CreateInstance<T>(sp, n));
    }
}