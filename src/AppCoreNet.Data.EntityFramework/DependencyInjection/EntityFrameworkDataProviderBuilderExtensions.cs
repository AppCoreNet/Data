// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Data.EntityFramework;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register a <see cref="EntityFrameworkDataProvider{TDbContext}"/>.
/// </summary>
public static class EntityFrameworkDataProviderBuilderExtensions
{
    /// <summary>
    /// Registers a <see cref="System.Data.Entity.DbContext"/> data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="System.Data.Entity.DbContext"/> on your own.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkDataProviderBuilder<TDbContext> AddEntityFramework<TDbContext>(
        this IDataProviderBuilder builder,
        string name,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : System.Data.Entity.DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddOptions();
        builder.Services.AddLogging();

        builder.AddProvider<EntityFrameworkDataProvider<TDbContext>>(
            name,
            providerLifetime,
            static (sp, name) =>
            {
                EntityFrameworkDataProviderServices<TDbContext> services =
                    EntityFrameworkDataProviderServices<TDbContext>.Create(name, sp);

                return new EntityFrameworkDataProvider<TDbContext>(name, services);
            });

        return new EntityFrameworkDataProviderBuilder<TDbContext>(name, builder.Services, providerLifetime);
    }

    /// <summary>
    /// Registers a <see cref="System.Data.Entity.DbContext"/> default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="System.Data.Entity.DbContext"/> on your own.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="System.Data.Entity.DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="EntityFrameworkDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkDataProviderBuilder<TDbContext> AddEntityFramework<TDbContext>(
        this IDataProviderBuilder builder,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : System.Data.Entity.DbContext
    {
        return builder.AddEntityFramework<TDbContext>(string.Empty, providerLifetime);
    }
}