// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCoreNet.Data;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IDataProvidersBuilder"/>.
/// </summary>
public static class DataProvidersBuilderExtensions
{
    private static ProviderRegistration? FindRegistration(IServiceCollection services, string name)
    {
        IEnumerable<ServiceDescriptor> registrations =
            services.Where(sd => sd.ServiceType == typeof(ProviderRegistration));

        return (ProviderRegistration?)registrations
                                      .FirstOrDefault(
                                          r => ((ProviderRegistration)r.ImplementationInstance!).Name == name)
                                      ?.ImplementationInstance;
    }

    /// <summary>
    /// Registers a data provider with a factory.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="lifetime">The lifetime of the data provider service.</param>
    /// <param name="factory">The factory method that creates the data provider.</param>
    /// <typeparam name="T">The type of the data provider.</typeparam>
    /// <returns>The <see cref="IDataProvidersBuilder"/> to allow chaining.</returns>
    public static IDataProvidersBuilder AddProvider<T>(
        this IDataProvidersBuilder builder,
        string name,
        ServiceLifetime lifetime,
        Func<IServiceProvider, string, T> factory)
        where T : class, IDataProvider
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(factory);

        ProviderRegistration? registration = FindRegistration(builder.Services, name);
        if (registration != null)
        {
            if (registration.ProviderType != typeof(T))
            {
                throw new InvalidOperationException(
                    $"Data provider with name '{name}' is already registered with type '{typeof(T).GetDisplayName()}'.");
            }
        }
        else
        {
            builder.Services.AddSingleton(new ProviderRegistration(name, typeof(T)));

#if !NET8_0_OR_GREATER
            builder.Services.Add(
                ServiceDescriptor.Describe(
                    typeof(T),
                    sp => factory(sp, name),
                    lifetime));

            builder.Services.AddTransient<IDataProvider>(
                sp => sp.GetRequiredService<IDataProviderResolver>()
                        .Resolve(name));
#else
            builder.Services.Add(
                ServiceDescriptor.DescribeKeyed(
                    typeof(T),
                    name,
                    (sp, key) => factory(sp, (string)key!),
                    lifetime));

            builder.Services.AddKeyedTransient<IDataProvider>(
                name,
                (sp, key) => sp.GetRequiredKeyedService<T>(key));

            builder.Services.AddTransient<T>(sp => sp.GetRequiredKeyedService<T>(name));
            builder.Services.AddTransient<IDataProvider>(sp => sp.GetRequiredKeyedService<T>(name));
#endif
        }

        return builder;
    }

    /// <summary>
    /// Registers a data provider.
    /// </summary>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="lifetime">The lifetime of the data provider service.</param>
    /// <typeparam name="T">The type of the data provider.</typeparam>
    /// <returns>The <see cref="IDataProvidersBuilder"/> to allow chaining.</returns>
    public static IDataProvidersBuilder AddProvider<T>(
        this IDataProvidersBuilder builder,
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