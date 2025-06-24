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
public static class EntityFrameworkCoreDataProviderBuilderExtensions
{
    private const int DefaultPoolSize = 128;

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
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    private static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCoreBase<TDbContext>(
        this IDataProviderBuilder builder,
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
            static (sp, name) =>
            {
                DbContextDataProviderServices<TDbContext> services =
                    DbContextDataProviderServices<TDbContext>.Create(name, sp);

                return new DbContextDataProvider<TDbContext>(name, services);
            });

        return new EntityFrameworkCoreDataProviderBuilder<TDbContext>(name, builder.Services, providerLifetime);
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
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    private static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCoreBase<TDbContext>(
        this IDataProviderBuilder builder,
        ServiceLifetime providerLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCoreBase<TDbContext>(string.Empty, providerLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCore<TDbContext>(
        this IDataProviderBuilder builder,
        string name,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        builder.Services.AddDbContext<TDbContext>(optionsAction, contextLifetime, optionsLifetime);
        return builder.AddEntityFrameworkCoreBase<TDbContext>(name, contextLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCore<TDbContext>(
        this IDataProviderBuilder builder,
        string name,
        Action<DbContextOptionsBuilder>? optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCore<TDbContext>(
            name,
            (_, ob) => optionsAction?.Invoke(ob),
            contextLifetime,
            optionsLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCore<TDbContext>(
        this IDataProviderBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCore<TDbContext>(string.Empty, optionsAction, contextLifetime, optionsLifetime);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="contextLifetime">The lifetime of the <see cref="DbContext"/>.</param>
    /// <param name="optionsLifetime">The lifetime of the <see cref="DbContextOptions"/>.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCore<TDbContext>(
        this IDataProviderBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCore<TDbContext>(string.Empty, optionsAction, contextLifetime, optionsLifetime);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCorePool<TDbContext>(
        this IDataProviderBuilder builder,
        string name,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);
        Ensure.Arg.NotNull(name);

        optionsAction ??= (_, _) => { };

        builder.Services.AddDbContextPool<TDbContext>(optionsAction, poolSize);
        return builder.AddEntityFrameworkCoreBase<TDbContext>(name);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="name">The name of the data provider.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCorePool<TDbContext>(
        this IDataProviderBuilder builder,
        string name,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCorePool<TDbContext>(
            name,
            (_, ob) => optionsAction?.Invoke(ob),
            poolSize);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCorePool<TDbContext>(
        this IDataProviderBuilder builder,
        Action<IServiceProvider, DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCorePool<TDbContext>(string.Empty, optionsAction, poolSize);
    }

    /// <summary>
    /// Registers a pooled <see cref="DbContext"/> with a default data provider in the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <param name="optionsAction">The delegate used to configure the options.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <returns>The <see cref="EntityFrameworkCoreDataProviderBuilder{TDbContext}"/>.</returns>
    public static EntityFrameworkCoreDataProviderBuilder<TDbContext> AddEntityFrameworkCorePool<TDbContext>(
        this IDataProviderBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        int poolSize = DefaultPoolSize)
        where TDbContext : DbContext
    {
        return builder.AddEntityFrameworkCorePool<TDbContext>(string.Empty, optionsAction, poolSize);
    }
}