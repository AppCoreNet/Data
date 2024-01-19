using System;
using System.Linq;
using AppCore.Data;
using AppCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IDataProvidersBuilder"/>.
/// </summary>
public static class DataProvidersBuilderExtensions
{
    private static DataProviderResolverOptions GetOptions(IServiceCollection services)
    {
        var options = (DataProviderResolverOptions?)
            services.FirstOrDefault(sd => sd.ServiceType == typeof(DataProviderResolverOptions))
                    ?.ImplementationInstance;

        if (options == null)
        {
            options = new DataProviderResolverOptions();
            services.AddSingleton(options);
        }

        return options;
    }

    /// <summary>
    /// Registers a data provider with a factory.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="lifetime">The lifetime of the data provider service.</param>
    /// <param name="factory">The factory method that creates the data provider.</param>
    /// <typeparam name="T">The type of the data provider.</typeparam>
    /// <returns>The <see cref="IDataProvidersBuilder"/>.</returns>
    public static IDataProvidersBuilder AddProvider<T>(
        this IDataProvidersBuilder builder,
        string name,
        ServiceLifetime lifetime,
        Func<IServiceProvider, T> factory)
        where T : class, IDataProvider
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);
        Ensure.Arg.NotNull(factory);

        DataProviderResolverOptions options = GetOptions(builder.Services);

        if (options.ProviderMap.TryGetValue(name, out Type providerType))
        {
            if (providerType != typeof(T))
            {
                throw new InvalidOperationException(
                    $"Data provider with name '{name}' is already registered with type '{typeof(T).GetDisplayName()}'.");
            }
        }
        else
        {
            options.ProviderMap.Add(name, typeof(T));
            builder.Services.Add(ServiceDescriptor.Describe(typeof(T), factory, lifetime));
        }

        if (name == string.Empty)
        {
            builder.Services.TryAddTransient<IDataProvider>(
                sp => sp.GetRequiredService<IDataProviderResolver>()
                        .Resolve(string.Empty));
        }

        return builder;
    }

    /// <summary>
    /// Registers a data provider.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="lifetime">The lifetime of the data provider service.</param>
    /// <typeparam name="T">The type of the data provider.</typeparam>
    /// <returns>The <see cref="IDataProvidersBuilder"/>.</returns>
    public static IDataProvidersBuilder AddProvider<T>(
        this IDataProvidersBuilder builder,
        string name,
        ServiceLifetime lifetime)
        where T : class, IDataProvider
    {
        return builder.AddProvider<T>(
            name,
            lifetime,
            sp => ActivatorUtilities.CreateInstance<T>(sp, name));
    }
}