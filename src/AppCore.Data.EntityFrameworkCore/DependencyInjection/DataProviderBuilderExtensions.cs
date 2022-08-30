// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.Data;
using AppCore.Data.EntityFrameworkCore;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods to register a <see cref="DbContext"/> data provider.
/// </summary>
public static class DataProviderBuilderExtensions
{
    /// <summary>
    /// Registers a <see cref="DbContext"/> data provider with the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TTag">The data provider tag.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <returns>The <see cref="IDbContextDataBuilder{TDbContext}"/>.</returns>
    public static IDbContextDataBuilder<TDbContext> AddDbContext<TTag, TDbContext>(this IDataProviderBuilder builder)
        where TDbContext : DbContext
    {
        Ensure.Arg.NotNull(builder);

        IServiceCollection services = builder.Services;

        services.TryAddScoped<IDbContextDataProvider<TDbContext>, DbContextDataProvider<TTag, TDbContext>>();

        services.TryAddEnumerable(
            new[]
            {
                ServiceDescriptor.Scoped<IDataProvider, IDbContextDataProvider<TDbContext>>(
                    sp => sp.GetRequiredService<IDbContextDataProvider<TDbContext>>()),
                ServiceDescriptor.Scoped<ITransactionManager, DbContextTransactionManager>(
                    sp => (DbContextTransactionManager)
                        sp.GetRequiredService<IDbContextDataProvider<TDbContext>>()
                          .TransactionManager)
            });

        return new DbContextDataBuilder<TDbContext>(services);
    }

    /// <summary>
    /// Registers a <see cref="DbContext"/> data provider with the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="builder">The <see cref="IDataProviderBuilder"/>.</param>
    /// <returns>The <see cref="IDbContextDataBuilder{TDbContext}"/>.</returns>
    public static IDbContextDataBuilder<TDbContext> AddDbContext<TDbContext>(this IDataProviderBuilder builder)
        where TDbContext : DbContext
    {
        return AddDbContext<DefaultDataProvider, TDbContext>(builder);
    }
}