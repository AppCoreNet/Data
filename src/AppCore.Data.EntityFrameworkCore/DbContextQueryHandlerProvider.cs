// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Provides the default implementation of <see cref="IDbContextQueryHandlerProvider{TDbContext}"/>.
/// </summary>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
public class DbContextQueryHandlerProvider<TDbContext> : IDbContextQueryHandlerProvider<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes an instance of the <see cref="DbContextQueryHandlerProvider{TDbContext}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    public DbContextQueryHandlerProvider(IServiceProvider serviceProvider)
    {
        Ensure.Arg.NotNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IDbContextQueryHandler<TEntity, TResult, TDbContext> GetHandler<TEntity, TResult>(Type queryType)
        where TEntity : IEntity
    {
        Ensure.Arg.NotNull(queryType);

        IEnumerable<IDbContextQueryHandler<TEntity, TResult, TDbContext>> handlers =
            _serviceProvider.GetServices<IDbContextQueryHandler<TEntity, TResult, TDbContext>>();

        IDbContextQueryHandler<TEntity, TResult, TDbContext>? handler =
            handlers.FirstOrDefault(h => queryType.IsAssignableFrom(h.QueryType));

        if (handler == null)
            throw new InvalidOperationException(
                $"There is no query handler for type {queryType.GetDisplayName()} registered.");

        return handler;
    }
}