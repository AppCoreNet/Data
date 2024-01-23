// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.EntityFrameworkCore;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register a <see cref="DbContextDataProvider{TDbContext}"/>.
/// </summary>
public static class DbContextDataProviderBuilderExtensions
{
    private const int DefaultPoolSize = 128;

    /// <summary>
    /// Registers a <see cref="DbContext"/> data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="DbContext"/> on your own.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContextCore<TDbContext>(
        this IDataProvidersBuilder builder,
        string name,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddOptions();
        builder.Services.AddLogging();

        builder.AddProvider<DbContextDataProvider<TDbContext>>(
            name,
            providerLifetime,
            sp =>
            {
                DbContextDataProviderServices<TDbContext> services =
                    DbContextDataProviderServices<TDbContext>.Create(name, sp);

                var logger = sp.GetRequiredService<ILogger<DbContextDataProvider<TDbContext>>>();

                return new DbContextDataProvider<TDbContext>(name, services, logger);
            });

        return new DbContextDataProviderBuilder<TDbContext>(name, builder.Services, providerLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    /// Note that you have to register the <see cref="DbContext"/> on your own.
    /// </remarks>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="providerLifetime">The lifetime of the data provider.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContextCore<TDbContext>(
        this IDataProvidersBuilder builder,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddDbContextCore<TDbContext>(string.Empty, providerLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContext<TDbContext>(
        this IDataProvidersBuilder builder,
        string name,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddDbContext<TDbContext>(optionsAction, contextLifetime, optionsLifetime);
        return builder.AddDbContextCore<TDbContext>(name, contextLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContext<TDbContext>(
        this IDataProvidersBuilder builder,
        string name,
        Action<DbContextOptionsBuilder>? optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddDbContext<TDbContext>(
            (_, ob) => optionsAction?.Invoke(ob),
            contextLifetime,
            optionsLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContext<TDbContext>(
        this IDataProvidersBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddDbContext<TDbContext>(string.Empty, optionsAction, contextLifetime, optionsLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContext<TDbContext>(
        this IDataProvidersBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddDbContext<TDbContext>(string.Empty, optionsAction, contextLifetime, optionsLifetime);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContextPool<TDbContext>(
        this IDataProvidersBuilder builder,
        string name,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        optionsAction ??= (_, _) => { };

        builder.Services.AddDbContextPool<TDbContext>(optionsAction, poolSize);
        return builder.AddDbContextCore<TDbContext>(name);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContextPool<TDbContext>(
        this IDataProvidersBuilder builder,
        string name,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        return builder.AddDbContextPool<TDbContext>(
            name,
            (_, ob) => optionsAction?.Invoke(ob),
            poolSize);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContextPool<TDbContext>(
        this IDataProvidersBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        return builder.AddDbContextPool<TDbContext>(string.Empty, optionsAction, poolSize);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProvidersBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="DbContextDataProviderBuilder{TDbContext}"/>.</returns>
    public static DbContextDataProviderBuilder<TDbContext> AddDbContextPool<TDbContext>(
        this IDataProvidersBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        return builder.AddDbContextPool<TDbContext>(string.Empty, optionsAction, poolSize);
    }
}