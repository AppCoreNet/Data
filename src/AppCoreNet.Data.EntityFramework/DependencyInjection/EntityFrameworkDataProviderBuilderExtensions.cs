// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Data.Entity;
using AppCoreNet.Data.EntityFramework;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    /// <remarks>
    /// Note that you have to register the <see cref="DbContext"/> on your own.
    /// </remarks>
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

        builder.Services.AddOptions();
        builder.Services.AddLogging();

        builder.Services.TryAdd(ServiceDescriptor.Describe(typeof(TDbContext), typeof(TDbContext), providerLifetime));

        builder.AddProvider<DbContextDataProvider<TDbContext>>(
            name,
            providerLifetime,
            static (sp, name) =>
            {
                DbContextDataProviderServices<TDbContext> services =
                    DbContextDataProviderServices<TDbContext>.Create(name, sp);

                return new DbContextDataProvider<TDbContext>(name, services);
            });

        return new EntityFrameworkDataProviderBuilder<TDbContext>(name, builder.Services, providerLifetime);
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