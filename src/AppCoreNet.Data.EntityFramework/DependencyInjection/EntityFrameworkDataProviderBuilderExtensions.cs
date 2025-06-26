// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Data.Entity;
using AppCoreNet.Data;
using AppCoreNet.Data.EntityFramework;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register a <see cref="DbContextDataProvider{TDbContext}"/>.
/// </summary>
public static class EntityFrameworkDataProviderBuilderExtensions
{
    /// <summary>
    /// Registers a <see cref="DbContext"/> data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkDataProviderBuilder<TDbContext> AddEntityFramework<TDbContext>(
        this IDataProviderBuilder builder,
        string name,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        IServiceCollection services = builder.Services;

        services.AddOptions();
        services.AddLogging();

        services.TryAdd(
            ServiceDescriptor.Describe(
                typeof(TDbContext),
                typeof(TDbContext),
                providerLifetime));

        services.TryAdd(
            ServiceDescriptor.Describe(
                typeof(DbContextTransactionManager<TDbContext>),
                typeof(DbContextTransactionManager<TDbContext>),
                providerLifetime));

        services.TryAdd(
            ServiceDescriptor.DescribeKeyed(
                typeof(DbContextQueryHandlerFactory<TDbContext>),
                name,
                static (sp, name) =>
                {
                    DbContextDataProviderOptions options = GetOptions(sp, (string)name!);
                    return new DbContextQueryHandlerFactory<TDbContext>(sp, options.QueryHandlerTypes);
                },
                providerLifetime));

        builder.AddProvider<DbContextDataProvider<TDbContext>>(
            name,
            providerLifetime,
            static (sp, name) =>
            {
                DbContextDataProviderOptions options = GetOptions(sp, name);

                return new DbContextDataProvider<TDbContext>(
                    name,
                    sp.GetRequiredService<TDbContext>(),
                    options.EntityMapperFactory(sp),
                    options.TokenGeneratorFactory(sp),
                    sp.GetRequiredKeyedService<DbContextQueryHandlerFactory<TDbContext>>(name),
                    sp.GetRequiredService<DbContextTransactionManager<TDbContext>>(),
                    sp.GetRequiredService<DataProviderLogger<DbContextDataProvider<TDbContext>>>());
            });

        return new EntityFrameworkDataProviderBuilder<TDbContext>(name, services, providerLifetime);

        static DbContextDataProviderOptions GetOptions(IServiceProvider sp, string name)
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<DbContextDataProviderOptions>>();
            return optionsMonitor.Get(name);
        }
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="DbContext"/> on your own.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkDataProviderBuilder<TDbContext> AddEntityFramework<TDbContext>(
        this IDataProviderBuilder builder,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddEntityFramework<TDbContext>(string.Empty, providerLifetime);
    }
}